using System;

namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// Plugin status information.
/// </summary>
public class PluginStatus
{
    /// <summary>
    /// Gets or sets a value indicating whether yt-dlp is available.
    /// </summary>
    public bool YtDlpAvailable { get; set; }

    /// <summary>
    /// Gets or sets the yt-dlp version.
    /// </summary>
    public string? YtDlpVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether sync is running.
    /// </summary>
    public bool IsSyncing { get; set; }

    /// <summary>
    /// Gets or sets the last sync time.
    /// </summary>
    public DateTime? LastSyncTime { get; set; }

    /// <summary>
    /// Gets or sets the expected binary path (for debugging).
    /// </summary>
    public string? BinaryPath { get; set; }
}
