using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CA1002, CA2227 // DTOs for serialization require List and setters

namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// Playlist metadata extracted from yt-dlp JSON output.
/// </summary>
public class PlaylistMetadata
{
    /// <summary>
    /// Gets or sets the entry type.
    /// </summary>
    [JsonPropertyName("_type")]
    public string? Type { get; set; }

    /// <summary>
    /// Gets or sets the playlist ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the playlist title.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Gets or sets the channel name.
    /// </summary>
    [JsonPropertyName("channel")]
    public string? Channel { get; set; }

    /// <summary>
    /// Gets or sets the channel ID.
    /// </summary>
    [JsonPropertyName("channel_id")]
    public string? ChannelId { get; set; }

    /// <summary>
    /// Gets or sets the uploader name.
    /// </summary>
    [JsonPropertyName("uploader")]
    public string? Uploader { get; set; }

    /// <summary>
    /// Gets or sets the uploader ID.
    /// </summary>
    [JsonPropertyName("uploader_id")]
    public string? UploaderId { get; set; }

    /// <summary>
    /// Gets or sets the playlist entry count.
    /// </summary>
    [JsonPropertyName("playlist_count")]
    public int? PlaylistCount { get; set; }

    /// <summary>
    /// Gets or sets the playlist entries.
    /// </summary>
    [JsonPropertyName("entries")]
    public List<VideoMetadata>? Entries { get; set; }

    /// <summary>
    /// Gets a value indicating whether this is a playlist.
    /// </summary>
    [JsonIgnore]
    public bool IsPlaylist => Type == "playlist";
}
