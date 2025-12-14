using System.Collections.Generic;

#pragma warning disable CA1002, CA2227 // DTOs for serialization require List and setters

namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// Configuration export/import format.
/// </summary>
public class PluginConfigExport
{
    /// <summary>
    /// Gets or sets the sources.
    /// </summary>
    public List<Source>? Sources { get; set; }

    /// <summary>
    /// Gets or sets the channel overrides.
    /// </summary>
    public List<ChannelOverride>? ChannelOverrides { get; set; }

    /// <summary>
    /// Gets or sets the global rules.
    /// </summary>
    public List<Rule>? GlobalRules { get; set; }
}
