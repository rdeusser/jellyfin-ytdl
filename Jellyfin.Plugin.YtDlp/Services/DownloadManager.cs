using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.YtDlp.Models;
using Microsoft.Extensions.Logging;

#pragma warning disable CA1002, CA2227 // DTOs for serialization require List and setters

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Manages video downloads with retry logic and queue management.
/// </summary>
public class DownloadManager : IDownloadManager
{
    private const int MaxRetries = 3;
    private static readonly int[] RetryDelaysSeconds = [5, 15, 45];
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    private readonly IYtDlpWrapper _ytDlp;
    private readonly IFileOrganizer _fileOrganizer;
    private readonly IMetadataMapper _metadataMapper;
    private readonly ILogger<DownloadManager> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DownloadManager"/> class.
    /// </summary>
    /// <param name="ytDlp">The yt-dlp wrapper.</param>
    /// <param name="fileOrganizer">The file organizer.</param>
    /// <param name="metadataMapper">The metadata mapper.</param>
    /// <param name="logger">Logger instance.</param>
    public DownloadManager(
        IYtDlpWrapper ytDlp,
        IFileOrganizer fileOrganizer,
        IMetadataMapper metadataMapper,
        ILogger<DownloadManager> logger)
    {
        _ytDlp = ytDlp;
        _fileOrganizer = fileOrganizer;
        _metadataMapper = metadataMapper;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> DownloadVideoAsync(VideoMetadata video, CancellationToken cancellationToken = default)
    {
        var config = Plugin.Instance?.Configuration;
        if (config == null || string.IsNullOrEmpty(config.DownloadPath))
        {
            _logger.LogError("Plugin not configured");
            return false;
        }

        var archivePath = GetArchivePath();
        if (_ytDlp.IsInArchive(archivePath, video.Id))
        {
            _logger.LogDebug("Video {VideoId} already in archive, skipping", video.Id);
            return true;
        }

        var destinationPath = _fileOrganizer.GetDestinationPath(video, config.DownloadPath);
        Directory.CreateDirectory(destinationPath);

        var channelName = _fileOrganizer.GetChannelDisplayName(
            video.ChannelId ?? "Unknown",
            video.Channel ?? "Unknown");

        await _metadataMapper.WriteShowNfoFileAsync(
            channelName,
            video.ChannelId ?? "Unknown",
            Path.Combine(config.DownloadPath, _fileOrganizer.SanitizeName(channelName)),
            cancellationToken).ConfigureAwait(false);

        var options = new DownloadOptions
        {
            SubtitleLanguages = config.SubtitleLanguages,
            IncludeAutoGenSubs = config.IncludeAutoGenSubs,
            SubtitleFormat = config.SubtitleFormat,
            ThumbnailFormat = config.ThumbnailFormat,
            ArchivePath = archivePath
        };

        string? lastError = null;

        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                var filePath = await _ytDlp.DownloadVideoAsync(
                    video.Id,
                    destinationPath,
                    config.FormatString,
                    options,
                    cancellationToken).ConfigureAwait(false);

                if (!string.IsNullOrEmpty(filePath))
                {
                    await _metadataMapper.WriteNfoFileAsync(video, filePath, cancellationToken).ConfigureAwait(false);
                    _logger.LogInformation("Downloaded {Title} to {Path}", video.Title, filePath);
                    return true;
                }

                lastError = "Download returned no file path";
            }
            catch (Exception ex)
            {
                lastError = ex.Message;
                _logger.LogWarning(ex, "Download attempt {Attempt} failed for {VideoId}", attempt + 1, video.Id);
            }

            if (attempt < MaxRetries - 1)
            {
                var delay = RetryDelaysSeconds[attempt];
                _logger.LogInformation("Retrying in {Delay} seconds...", delay);
                await Task.Delay(TimeSpan.FromSeconds(delay), cancellationToken).ConfigureAwait(false);
            }
        }

        await AddToFailedQueueAsync(video, lastError ?? "Unknown error", cancellationToken).ConfigureAwait(false);
        return false;
    }

    /// <inheritdoc />
    public async Task ProcessQueueAsync(CancellationToken cancellationToken = default)
    {
        var queuePath = GetQueuePath();
        if (!File.Exists(queuePath))
        {
            return;
        }

        var json = await File.ReadAllTextAsync(queuePath, cancellationToken).ConfigureAwait(false);
        var queue = JsonSerializer.Deserialize<DownloadQueue>(json);

        if (queue?.Failed == null || queue.Failed.Count == 0)
        {
            return;
        }

        var remaining = new List<FailedDownload>();

        foreach (var failed in queue.Failed)
        {
            var video = new VideoMetadata
            {
                Id = failed.VideoId,
                ChannelId = failed.ChannelId
            };

            var success = await DownloadVideoAsync(video, cancellationToken).ConfigureAwait(false);
            if (!success)
            {
                failed.Attempts++;
                failed.LastAttempt = DateTime.UtcNow;
                remaining.Add(failed);
            }
        }

        queue.Failed = remaining;
        var updatedJson = JsonSerializer.Serialize(queue, JsonOptions);
        await File.WriteAllTextAsync(queuePath, updatedJson, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public string GetStateDirectory()
    {
        var config = Plugin.Instance?.Configuration;
        var basePath = config?.DownloadPath ?? string.Empty;
        return Path.Combine(basePath, ".ytdl-state");
    }

    /// <inheritdoc />
    public string GetArchivePath()
    {
        return Path.Combine(GetStateDirectory(), "archive.txt");
    }

    private string GetQueuePath()
    {
        return Path.Combine(GetStateDirectory(), "queue.json");
    }

    private async Task AddToFailedQueueAsync(VideoMetadata video, string error, CancellationToken cancellationToken)
    {
        var queuePath = GetQueuePath();
        var stateDir = GetStateDirectory();

        Directory.CreateDirectory(stateDir);

        DownloadQueue queue;

        if (File.Exists(queuePath))
        {
            var json = await File.ReadAllTextAsync(queuePath, cancellationToken).ConfigureAwait(false);
            queue = JsonSerializer.Deserialize<DownloadQueue>(json) ?? new DownloadQueue();
        }
        else
        {
            queue = new DownloadQueue();
        }

        queue.Failed ??= [];

        queue.Failed.Add(new FailedDownload
        {
            VideoId = video.Id,
            Url = $"https://www.youtube.com/watch?v={video.Id}",
            ChannelId = video.ChannelId ?? string.Empty,
            Attempts = MaxRetries,
            LastAttempt = DateTime.UtcNow,
            LastError = error
        });

        var updatedJson = JsonSerializer.Serialize(queue, JsonOptions);
        await File.WriteAllTextAsync(queuePath, updatedJson, cancellationToken).ConfigureAwait(false);
    }

    private sealed class DownloadQueue
    {
        public List<FailedDownload>? Failed { get; set; }
    }
}
