using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Jellyfin.Plugin.YtDlp.Models;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Evaluates rules to determine video organization paths.
/// </summary>
public class RuleEngine : IRuleEngine
{
    /// <inheritdoc />
    public string? EvaluateRules(VideoMetadata video, IEnumerable<Rule> rules)
    {
        var sortedRules = rules.OrderByDescending(r => r.Priority).ToList();

        foreach (var rule in sortedRules)
        {
            if (AllConditionsMatch(video, rule.Conditions))
            {
                var path = rule.FolderName;

                if (rule.Children.Count > 0)
                {
                    var childPath = EvaluateRules(video, rule.Children);
                    if (!string.IsNullOrEmpty(childPath))
                    {
                        path = Path.Combine(path, childPath);
                    }
                }

                return path;
            }
        }

        return null;
    }

    /// <inheritdoc />
    public bool EvaluateCondition(VideoMetadata video, Condition condition)
    {
        var fieldValue = GetFieldValue(video, condition.Field);

        if (string.IsNullOrEmpty(fieldValue))
        {
            return condition.Operator is ConditionOperator.NotContains or ConditionOperator.NotEquals;
        }

        var comparison = StringComparison.OrdinalIgnoreCase;
        var value = condition.Value;

        return condition.Operator switch
        {
            ConditionOperator.Contains => fieldValue.Contains(value, comparison),
            ConditionOperator.StartsWith => fieldValue.StartsWith(value, comparison),
            ConditionOperator.EndsWith => fieldValue.EndsWith(value, comparison),
            ConditionOperator.Equals => fieldValue.Equals(value, comparison),
            ConditionOperator.NotContains => !fieldValue.Contains(value, comparison),
            ConditionOperator.NotEquals => !fieldValue.Equals(value, comparison),
            _ => false
        };
    }

    private bool AllConditionsMatch(VideoMetadata video, IEnumerable<Condition> conditions)
    {
        return conditions.All(c => EvaluateCondition(video, c));
    }

    private static string? GetFieldValue(VideoMetadata video, ConditionField field)
    {
        return field switch
        {
            ConditionField.Title => video.Title,
            ConditionField.Description => video.Description,
            ConditionField.Tags => video.Tags != null ? string.Join(" ", video.Tags) : null,
            ConditionField.Category => video.Categories != null ? string.Join(" ", video.Categories) : null,
            _ => null
        };
    }
}
