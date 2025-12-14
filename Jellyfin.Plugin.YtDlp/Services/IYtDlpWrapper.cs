using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.YtDlp.Models;

#pragma warning disable CA1819 // DTOs for serialization require arrays

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Wraps yt-dlp CLI operations.
/// </summary>
public interface IYtDlpWrapper
{
    /// <summary>
    /// Fetches metadata for a URL without downloading.
    /// </summary>
    /// <param name="url">The YouTube URL.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The playlist or video metadata.</returns>
    Task<PlaylistMetadata?> FetchMetadataAsync(string url, CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads a video with the specified options.
    /// </summary>
    /// <param name="videoId">The video ID.</param>
    /// <param name="outputPath">The output directory.</param>
    /// <param name="formatString">The format selection string.</param>
    /// <param name="options">Additional download options.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The path to the downloaded file.</returns>
    Task<string?> DownloadVideoAsync(
        string videoId,
        string outputPath,
        string formatString,
        DownloadOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a video is in the archive.
    /// </summary>
    /// <param name="archivePath">Path to the archive file.</param>
    /// <param name="videoId">The video ID.</param>
    /// <returns>True if already downloaded.</returns>
    bool IsInArchive(string archivePath, string videoId);

    /// <summary>
    /// Adds a video to the archive.
    /// </summary>
    /// <param name="archivePath">Path to the archive file.</param>
    /// <param name="videoId">The video ID.</param>
    void AddToArchive(string archivePath, string videoId);
}

/// <summary>
/// Options for downloading a video.
/// </summary>
public class DownloadOptions
{
    /// <summary>
    /// Gets or sets the subtitle languages.
    /// </summary>
    public string[]? SubtitleLanguages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include auto-generated subtitles.
    /// </summary>
    public bool IncludeAutoGenSubs { get; set; }

    /// <summary>
    /// Gets or sets the subtitle format.
    /// </summary>
    public string? SubtitleFormat { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail format.
    /// </summary>
    public string? ThumbnailFormat { get; set; }

    /// <summary>
    /// Gets or sets the archive file path.
    /// </summary>
    public string? ArchivePath { get; set; }
}
