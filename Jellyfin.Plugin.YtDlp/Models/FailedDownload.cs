using System;

namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// A download that failed and is queued for retry.
/// </summary>
public class FailedDownload
{
    /// <summary>
    /// Gets or sets the YouTube video ID.
    /// </summary>
    public string VideoId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video URL.
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the channel ID.
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of retry attempts.
    /// </summary>
    public int Attempts { get; set; }

    /// <summary>
    /// Gets or sets when the last attempt occurred.
    /// </summary>
    public DateTime LastAttempt { get; set; }

    /// <summary>
    /// Gets or sets the last error message.
    /// </summary>
    public string LastError { get; set; } = string.Empty;
}
