using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Background service that handles scheduled syncing.
/// </summary>
public class SyncService : IHostedService, IDisposable
{
    private readonly IYtDlpWrapper _ytDlp;
    private readonly IDownloadManager _downloadManager;
    private readonly ILogger<SyncService> _logger;
    private Timer? _timer;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncService"/> class.
    /// </summary>
    /// <param name="ytDlp">The yt-dlp wrapper.</param>
    /// <param name="downloadManager">The download manager.</param>
    /// <param name="logger">Logger instance.</param>
    public SyncService(
        IYtDlpWrapper ytDlp,
        IDownloadManager downloadManager,
        ILogger<SyncService> logger)
    {
        _ytDlp = ytDlp;
        _downloadManager = downloadManager;
        _logger = logger;
    }

    /// <summary>
    /// Gets a value indicating whether a sync is currently running.
    /// </summary>
    public bool IsSyncing { get; private set; }

    /// <summary>
    /// Gets the last sync time.
    /// </summary>
    public DateTime? LastSyncTime { get; private set; }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sync service started");

        var config = Plugin.Instance?.Configuration;
        if (config?.ScheduleEnabled == true && config.ScheduleIntervalHours > 0)
        {
            var interval = TimeSpan.FromHours(config.ScheduleIntervalHours);
            _timer = new Timer(OnTimerElapsed, null, interval, interval);
            _logger.LogInformation("Scheduled sync enabled, interval: {Interval} hours", config.ScheduleIntervalHours);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sync service stopping");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Triggers a manual sync of all enabled sources.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    public async Task SyncAllAsync(CancellationToken cancellationToken = default)
    {
        if (IsSyncing)
        {
            _logger.LogWarning("Sync already in progress, skipping");
            return;
        }

        IsSyncing = true;

        try
        {
            _logger.LogInformation("Starting sync of all sources");

            var config = Plugin.Instance?.Configuration;
            if (config == null)
            {
                _logger.LogError("Plugin not configured");
                return;
            }

            var enabledSources = config.Sources.Where(s => s.Enabled).ToList();

            foreach (var source in enabledSources)
            {
                await SyncSourceAsync(source.Url, cancellationToken).ConfigureAwait(false);
                source.LastSyncedAt = DateTime.UtcNow;
            }

            await _downloadManager.ProcessQueueAsync(cancellationToken).ConfigureAwait(false);

            LastSyncTime = DateTime.UtcNow;
            _logger.LogInformation("Sync completed");
        }
        finally
        {
            IsSyncing = false;
        }
    }

    /// <summary>
    /// Syncs a single source URL.
    /// </summary>
    /// <param name="url">The source URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    public async Task SyncSourceAsync(string url, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing source: {Url}", url);

        var metadata = await _ytDlp.FetchMetadataAsync(url, cancellationToken).ConfigureAwait(false);

        if (metadata?.Entries == null || metadata.Entries.Count == 0)
        {
            _logger.LogWarning("No videos found for {Url}", url);
            return;
        }

        _logger.LogInformation("Found {Count} videos", metadata.Entries.Count);

        foreach (var video in metadata.Entries)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            await _downloadManager.DownloadVideoAsync(video, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes managed resources.
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            _timer?.Dispose();
        }

        _disposed = true;
    }

    private void OnTimerElapsed(object? state)
    {
        _ = SyncAllAsync(CancellationToken.None);
    }
}
