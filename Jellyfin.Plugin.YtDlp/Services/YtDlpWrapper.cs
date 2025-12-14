using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.YtDlp.Models;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Wraps yt-dlp CLI operations.
/// </summary>
public class YtDlpWrapper : IYtDlpWrapper
{
    private readonly IYtDlpBinaryManager _binaryManager;
    private readonly ILogger<YtDlpWrapper> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="YtDlpWrapper"/> class.
    /// </summary>
    /// <param name="binaryManager">Binary manager instance.</param>
    /// <param name="logger">Logger instance.</param>
    public YtDlpWrapper(IYtDlpBinaryManager binaryManager, ILogger<YtDlpWrapper> logger)
    {
        _binaryManager = binaryManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<PlaylistMetadata?> FetchMetadataAsync(string url, CancellationToken cancellationToken = default)
    {
        var binaryPath = await _binaryManager.GetBinaryPathAsync(cancellationToken).ConfigureAwait(false);

        var args = new List<string>
        {
            "--dump-json",
            "--flat-playlist",
            "--ignore-errors",
            url
        };

        var (exitCode, output, error) = await RunProcessAsync(binaryPath, args, cancellationToken).ConfigureAwait(false);

        if (exitCode != 0)
        {
            _logger.LogError("yt-dlp failed with exit code {ExitCode}: {Error}", exitCode, error);
            return null;
        }

        return ParseMetadataOutput(output);
    }

    /// <inheritdoc />
    public async Task<string?> DownloadVideoAsync(
        string videoId,
        string outputPath,
        string formatString,
        DownloadOptions options,
        CancellationToken cancellationToken = default)
    {
        var binaryPath = await _binaryManager.GetBinaryPathAsync(cancellationToken).ConfigureAwait(false);
        var videoUrl = $"https://www.youtube.com/watch?v={videoId}";

        var args = new List<string>
        {
            "--format", formatString,
            "--write-info-json",
            "--write-thumbnail",
            "--output", Path.Combine(outputPath, "%(id)s.%(ext)s"),
            "--no-progress"
        };

        if (!string.IsNullOrEmpty(options.ThumbnailFormat))
        {
            args.AddRange(["--convert-thumbnails", options.ThumbnailFormat]);
        }

        if (options.SubtitleLanguages?.Length > 0)
        {
            args.Add("--write-subs");
            args.AddRange(["--sub-langs", string.Join(",", options.SubtitleLanguages)]);

            if (options.IncludeAutoGenSubs)
            {
                args.Add("--write-auto-subs");
            }

            if (!string.IsNullOrEmpty(options.SubtitleFormat))
            {
                args.AddRange(["--sub-format", options.SubtitleFormat]);
            }
        }

        if (!string.IsNullOrEmpty(options.ArchivePath))
        {
            args.AddRange(["--download-archive", options.ArchivePath]);
        }

        args.Add(videoUrl);

        _logger.LogInformation("Downloading video {VideoId}", videoId);

        var (exitCode, output, error) = await RunProcessAsync(binaryPath, args, cancellationToken).ConfigureAwait(false);

        if (exitCode != 0)
        {
            _logger.LogError("Download failed for {VideoId}: {Error}", videoId, error);
            return null;
        }

        var expectedPath = Path.Combine(outputPath, $"{videoId}.mp4");
        if (File.Exists(expectedPath))
        {
            return expectedPath;
        }

        var files = Directory.GetFiles(outputPath, $"{videoId}.*");
        foreach (var file in files)
        {
            var ext = Path.GetExtension(file).ToLowerInvariant();
            if (ext is ".mp4" or ".mkv" or ".webm" or ".mov" or ".avi")
            {
                return file;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public bool IsInArchive(string archivePath, string videoId)
    {
        if (!File.Exists(archivePath))
        {
            return false;
        }

        var archiveEntry = $"youtube {videoId}";
        var content = File.ReadAllText(archivePath);
        return content.Contains(archiveEntry, StringComparison.Ordinal);
    }

    /// <inheritdoc />
    public void AddToArchive(string archivePath, string videoId)
    {
        var directory = Path.GetDirectoryName(archivePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.AppendAllText(archivePath, $"youtube {videoId}\n");
    }

    private static PlaylistMetadata? ParseMetadataOutput(string output)
    {
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        if (lines.Length == 0)
        {
            return null;
        }

        if (lines.Length == 1)
        {
            var video = JsonSerializer.Deserialize<VideoMetadata>(lines[0]);
            if (video == null)
            {
                return null;
            }

            return new PlaylistMetadata
            {
                Id = video.Id,
                Title = video.Title,
                Channel = video.Channel,
                ChannelId = video.ChannelId,
                Entries = [video]
            };
        }

        var entries = new List<VideoMetadata>();
        foreach (var line in lines)
        {
            var entry = JsonSerializer.Deserialize<VideoMetadata>(line);
            if (entry != null)
            {
                entries.Add(entry);
            }
        }

        return new PlaylistMetadata
        {
            Type = "playlist",
            Entries = entries,
            PlaylistCount = entries.Count
        };
    }

    private static async Task<(int ExitCode, string Output, string Error)> RunProcessAsync(
        string fileName,
        IEnumerable<string> args,
        CancellationToken cancellationToken)
    {
        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        foreach (var arg in args)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

        return (process.ExitCode, outputBuilder.ToString(), errorBuilder.ToString());
    }
}
