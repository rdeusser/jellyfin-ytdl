namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// A single condition for rule matching.
/// </summary>
public class Condition
{
    /// <summary>
    /// Gets or sets the metadata field to evaluate.
    /// </summary>
    public ConditionField Field { get; set; }

    /// <summary>
    /// Gets or sets the comparison operator.
    /// </summary>
    public ConditionOperator Operator { get; set; }

    /// <summary>
    /// Gets or sets the value to compare against.
    /// </summary>
    public string Value { get; set; } = string.Empty;
}
