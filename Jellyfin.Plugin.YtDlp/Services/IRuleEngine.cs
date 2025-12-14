using System.Collections.Generic;
using Jellyfin.Plugin.YtDlp.Models;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Evaluates rules to determine video organization paths.
/// </summary>
public interface IRuleEngine
{
    /// <summary>
    /// Evaluates rules against video metadata to determine the subfolder path.
    /// </summary>
    /// <param name="video">The video metadata.</param>
    /// <param name="rules">The rules to evaluate.</param>
    /// <returns>The relative subfolder path, or null if no rules match.</returns>
    string? EvaluateRules(VideoMetadata video, IEnumerable<Rule> rules);

    /// <summary>
    /// Checks if a single condition matches the video metadata.
    /// </summary>
    /// <param name="video">The video metadata.</param>
    /// <param name="condition">The condition to evaluate.</param>
    /// <returns>True if the condition matches.</returns>
    bool EvaluateCondition(VideoMetadata video, Condition condition);
}
