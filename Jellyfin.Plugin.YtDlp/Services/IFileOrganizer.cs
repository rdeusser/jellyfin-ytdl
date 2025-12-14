using Jellyfin.Plugin.YtDlp.Models;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Organizes downloaded files into the appropriate directory structure.
/// </summary>
public interface IFileOrganizer
{
    /// <summary>
    /// Gets the destination path for a video based on rules and channel settings.
    /// </summary>
    /// <param name="video">The video metadata.</param>
    /// <param name="basePath">The base download directory.</param>
    /// <returns>The full destination directory path.</returns>
    string GetDestinationPath(VideoMetadata video, string basePath);

    /// <summary>
    /// Gets the display name for a channel, applying any overrides.
    /// </summary>
    /// <param name="channelId">The YouTube channel ID.</param>
    /// <param name="defaultName">The default channel name from YouTube.</param>
    /// <returns>The display name to use.</returns>
    string GetChannelDisplayName(string channelId, string defaultName);

    /// <summary>
    /// Gets the effective channel ID, following any merge rules.
    /// </summary>
    /// <param name="channelId">The original channel ID.</param>
    /// <returns>The effective channel ID after applying merges.</returns>
    string GetEffectiveChannelId(string channelId);

    /// <summary>
    /// Sanitizes a string for use as a filename or directory name.
    /// </summary>
    /// <param name="name">The name to sanitize.</param>
    /// <returns>A filesystem-safe name.</returns>
    string SanitizeName(string name);
}
