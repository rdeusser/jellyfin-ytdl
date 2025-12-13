using System;
using System.Collections.Generic;

#pragma warning disable CA1002, CA2227 // DTOs for serialization require List and setters

namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// A YouTube channel or playlist to download from.
/// </summary>
public class Source
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the display name shown in the UI.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the YouTube URL (channel or playlist).
    /// </summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source type.
    /// </summary>
    public SourceType Type { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this source is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional format string override.
    /// </summary>
    public string? FormatString { get; set; }

    /// <summary>
    /// Gets or sets source-specific organization rules.
    /// </summary>
    public List<Rule> Rules { get; set; } = [];

    /// <summary>
    /// Gets or sets when this source was last synced.
    /// </summary>
    public DateTime? LastSyncedAt { get; set; }
}
