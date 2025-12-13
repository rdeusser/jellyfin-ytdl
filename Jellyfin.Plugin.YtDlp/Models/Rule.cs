using System;
using System.Collections.Generic;

#pragma warning disable CA1002, CA2227 // DTOs for serialization require List and setters

namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// A rule for organizing videos into subfolders.
/// </summary>
public class Rule
{
    /// <summary>
    /// Gets or sets the unique identifier.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the display name for this rule.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the subfolder name for matching videos.
    /// </summary>
    public string FolderName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the conditions that must all match (AND logic).
    /// </summary>
    public List<Condition> Conditions { get; set; } = [];

    /// <summary>
    /// Gets or sets nested child rules for hierarchical organization.
    /// </summary>
    public List<Rule> Children { get; set; } = [];

    /// <summary>
    /// Gets or sets the priority (higher values checked first).
    /// </summary>
    public int Priority { get; set; }
}
