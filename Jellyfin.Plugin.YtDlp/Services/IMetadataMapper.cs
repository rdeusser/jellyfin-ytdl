using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.YtDlp.Models;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Maps yt-dlp metadata to Jellyfin NFO format.
/// </summary>
public interface IMetadataMapper
{
    /// <summary>
    /// Creates a Jellyfin-compatible NFO file from video metadata.
    /// </summary>
    /// <param name="video">The video metadata.</param>
    /// <param name="videoFilePath">The path to the video file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    Task WriteNfoFileAsync(VideoMetadata video, string videoFilePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a Jellyfin-compatible tvshow.nfo file for a channel.
    /// </summary>
    /// <param name="channelName">The channel name.</param>
    /// <param name="channelId">The channel ID.</param>
    /// <param name="channelPath">The path to the channel directory.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    Task WriteShowNfoFileAsync(
        string channelName,
        string channelId,
        string channelPath,
        CancellationToken cancellationToken = default);
}
