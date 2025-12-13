using System.Collections.Generic;

#pragma warning disable CA1002, CA2227 // DTOs for serialization require List and setters

namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// Overrides for a specific YouTube channel.
/// </summary>
public class ChannelOverride
{
    /// <summary>
    /// Gets or sets the YouTube channel ID.
    /// </summary>
    public string ChannelId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the custom display name (overrides YouTube channel name).
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the channel ID to merge this channel into.
    /// </summary>
    public string? MergeIntoChannelId { get; set; }

    /// <summary>
    /// Gets or sets an optional format string override.
    /// </summary>
    public string? FormatString { get; set; }

    /// <summary>
    /// Gets or sets channel-specific organization rules.
    /// </summary>
    public List<Rule> Rules { get; set; } = [];
}
