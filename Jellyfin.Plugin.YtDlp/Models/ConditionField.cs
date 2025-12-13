namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// Defines which video metadata field a condition evaluates.
/// </summary>
public enum ConditionField
{
    /// <summary>
    /// The video title.
    /// </summary>
    Title,

    /// <summary>
    /// The video description.
    /// </summary>
    Description,

    /// <summary>
    /// The video tags.
    /// </summary>
    Tags,

    /// <summary>
    /// The video category.
    /// </summary>
    Category
}
