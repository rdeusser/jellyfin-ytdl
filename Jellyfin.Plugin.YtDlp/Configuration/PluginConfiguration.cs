using System.Collections.Generic;
using Jellyfin.Plugin.YtDlp.Models;
using MediaBrowser.Model.Plugins;

#pragma warning disable CA1002, CA1819, CA2227 // DTOs for serialization require List/array and setters

namespace Jellyfin.Plugin.YtDlp.Configuration;

/// <summary>
/// Plugin configuration for jellyfin-ytdl.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PluginConfiguration"/> class.
    /// </summary>
    public PluginConfiguration()
    {
        DownloadPath = string.Empty;
        YtDlpPath = string.Empty;
        ScheduleEnabled = false;
        ScheduleIntervalHours = 24;
        MaxConcurrentDownloads = 1;
        FormatString = "bestvideo*+bestaudio/best";
        SubtitleLanguages = ["en"];
        IncludeAutoGenSubs = false;
        SubtitleFormat = "srt";
        ThumbnailFormat = "jpg";
        DownloadChannelArt = true;
        Sources = [];
        ChannelOverrides = [];
        GlobalRules = [];
    }

    /// <summary>
    /// Gets or sets the base directory for downloaded videos.
    /// </summary>
    public string DownloadPath { get; set; }

    /// <summary>
    /// Gets or sets the path to yt-dlp binary (auto-managed if empty).
    /// </summary>
    public string YtDlpPath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether scheduled sync is enabled.
    /// </summary>
    public bool ScheduleEnabled { get; set; }

    /// <summary>
    /// Gets or sets the sync interval in hours.
    /// </summary>
    public int ScheduleIntervalHours { get; set; }

    /// <summary>
    /// Gets or sets the maximum concurrent downloads.
    /// </summary>
    public int MaxConcurrentDownloads { get; set; }

    /// <summary>
    /// Gets or sets the default yt-dlp format string.
    /// </summary>
    public string FormatString { get; set; }

    /// <summary>
    /// Gets or sets the subtitle languages to download.
    /// </summary>
    public string[] SubtitleLanguages { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to include auto-generated subtitles.
    /// </summary>
    public bool IncludeAutoGenSubs { get; set; }

    /// <summary>
    /// Gets or sets the subtitle format.
    /// </summary>
    public string SubtitleFormat { get; set; }

    /// <summary>
    /// Gets or sets the thumbnail format.
    /// </summary>
    public string ThumbnailFormat { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to download channel artwork.
    /// </summary>
    public bool DownloadChannelArt { get; set; }

    /// <summary>
    /// Gets or sets the list of content sources.
    /// </summary>
    public List<Source> Sources { get; set; }

    /// <summary>
    /// Gets or sets channel-specific overrides.
    /// </summary>
    public List<ChannelOverride> ChannelOverrides { get; set; }

    /// <summary>
    /// Gets or sets global organization rules.
    /// </summary>
    public List<Rule> GlobalRules { get; set; }
}
