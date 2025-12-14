using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Jellyfin.Plugin.YtDlp.Models;

namespace Jellyfin.Plugin.YtDlp.Services;

/// <summary>
/// Organizes downloaded files into the appropriate directory structure.
/// </summary>
public partial class FileOrganizer : IFileOrganizer
{
    private readonly IRuleEngine _ruleEngine;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileOrganizer"/> class.
    /// </summary>
    /// <param name="ruleEngine">The rule engine instance.</param>
    public FileOrganizer(IRuleEngine ruleEngine)
    {
        _ruleEngine = ruleEngine;
    }

    /// <inheritdoc />
    public string GetDestinationPath(VideoMetadata video, string basePath)
    {
        var config = Plugin.Instance?.Configuration;
        var channelId = video.ChannelId ?? "Unknown";
        var channelName = video.Channel ?? video.Uploader ?? "Unknown";

        var effectiveChannelId = GetEffectiveChannelId(channelId);
        var displayName = GetChannelDisplayName(effectiveChannelId, channelName);
        var sanitizedChannelName = SanitizeName(displayName);

        var path = Path.Combine(basePath, sanitizedChannelName);

        var allRules = config?.GlobalRules.ToList() ?? [];

        var channelOverride = config?.ChannelOverrides
            .FirstOrDefault(o => o.ChannelId == effectiveChannelId);

        if (channelOverride?.Rules.Count > 0)
        {
            allRules.AddRange(channelOverride.Rules);
        }

        var source = config?.Sources
            .FirstOrDefault(s => s.Enabled);

        if (source?.Rules.Count > 0)
        {
            allRules.AddRange(source.Rules);
        }

        if (allRules.Count > 0)
        {
            var subPath = _ruleEngine.EvaluateRules(video, allRules);
            if (!string.IsNullOrEmpty(subPath))
            {
                path = Path.Combine(path, subPath);
            }
        }

        return path;
    }

    /// <inheritdoc />
    public string GetChannelDisplayName(string channelId, string defaultName)
    {
        var config = Plugin.Instance?.Configuration;
        var effectiveId = GetEffectiveChannelId(channelId);

        var channelOverride = config?.ChannelOverrides
            .FirstOrDefault(o => o.ChannelId == effectiveId);

        return channelOverride?.DisplayName ?? defaultName;
    }

    /// <inheritdoc />
    public string GetEffectiveChannelId(string channelId)
    {
        var config = Plugin.Instance?.Configuration;

        var channelOverride = config?.ChannelOverrides
            .FirstOrDefault(o => o.ChannelId == channelId);

        if (!string.IsNullOrEmpty(channelOverride?.MergeIntoChannelId))
        {
            return channelOverride.MergeIntoChannelId;
        }

        return channelId;
    }

    /// <inheritdoc />
    public string SanitizeName(string name)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(name.Where(c => !invalidChars.Contains(c)).ToArray());

        sanitized = MultipleSpacesRegex().Replace(sanitized, " ").Trim();

        if (sanitized.Length > 100)
        {
            sanitized = sanitized[..100].TrimEnd();
        }

        return string.IsNullOrWhiteSpace(sanitized) ? "Unknown" : sanitized;
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpacesRegex();
}
