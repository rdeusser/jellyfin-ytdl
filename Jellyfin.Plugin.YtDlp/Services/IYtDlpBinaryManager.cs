using System.Threading;
using System.Threading.Tasks;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Manages the yt-dlp binary (download, update, path resolution).
/// </summary>
public interface IYtDlpBinaryManager
{
    /// <summary>
    /// Gets the path to the yt-dlp binary, downloading if necessary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The absolute path to the yt-dlp binary.</returns>
    Task<string> GetBinaryPathAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a value indicating whether yt-dlp is available.
    /// </summary>
    /// <returns>True if yt-dlp is available.</returns>
    bool IsAvailable();

    /// <summary>
    /// Gets the installed yt-dlp version.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The version string, or null if not installed.</returns>
    Task<string?> GetVersionAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads or updates yt-dlp to the latest version.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the async operation.</returns>
    Task UpdateAsync(CancellationToken cancellationToken = default);
}
