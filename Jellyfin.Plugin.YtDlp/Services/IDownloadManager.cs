using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.YtDlp.Models;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Manages video downloads with retry logic and queue management.
/// </summary>
public interface IDownloadManager
{
    /// <summary>
    /// Downloads a video with retry logic.
    /// </summary>
    /// <param name="video">The video metadata.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if download succeeded.</returns>
    Task<bool> DownloadVideoAsync(VideoMetadata video, CancellationToken cancellationToken = default);

    /// <summary>
    /// Processes the failed download queue.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    Task ProcessQueueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the path to the state directory.
    /// </summary>
    /// <returns>The state directory path.</returns>
    string GetStateDirectory();

    /// <summary>
    /// Gets the path to the archive file.
    /// </summary>
    /// <returns>The archive file path.</returns>
    string GetArchivePath();
}
