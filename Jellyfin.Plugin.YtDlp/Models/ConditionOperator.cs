namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// Defines the comparison operator for a condition.
/// </summary>
public enum ConditionOperator
{
    /// <summary>
    /// Field contains the value (case-insensitive).
    /// </summary>
    Contains,

    /// <summary>
    /// Field starts with the value (case-insensitive).
    /// </summary>
    StartsWith,

    /// <summary>
    /// Field ends with the value (case-insensitive).
    /// </summary>
    EndsWith,

    /// <summary>
    /// Field equals the value exactly (case-insensitive).
    /// </summary>
    Equals,

    /// <summary>
    /// Field does not contain the value (case-insensitive).
    /// </summary>
    NotContains,

    /// <summary>
    /// Field does not equal the value (case-insensitive).
    /// </summary>
    NotEquals
}
