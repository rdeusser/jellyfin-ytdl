using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#pragma warning disable CA1002, CA2227 // DTOs for serialization require List and setters

namespace Jellyfin.Plugin.YtDlp.Models;

/// <summary>
/// Video metadata extracted from yt-dlp JSON output.
/// </summary>
public class VideoMetadata
{
    /// <summary>
    /// Gets or sets the video ID.
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video title.
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the video description.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the upload date (YYYYMMDD format).
    /// </summary>
    [JsonPropertyName("upload_date")]
    public string? UploadDate { get; set; }

    /// <summary>
    /// Gets or sets the duration in seconds.
    /// </summary>
    [JsonPropertyName("duration")]
    public int? Duration { get; set; }

    /// <summary>
    /// Gets or sets the view count.
    /// </summary>
    [JsonPropertyName("view_count")]
    public long? ViewCount { get; set; }

    /// <summary>
    /// Gets or sets the like count.
    /// </summary>
    [JsonPropertyName("like_count")]
    public long? LikeCount { get; set; }

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
    /// Gets or sets the channel URL.
    /// </summary>
    [JsonPropertyName("channel_url")]
    public string? ChannelUrl { get; set; }

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
    /// Gets or sets the thumbnail URL.
    /// </summary>
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }

    /// <summary>
    /// Gets or sets the video categories.
    /// </summary>
    [JsonPropertyName("categories")]
    public List<string>? Categories { get; set; }

    /// <summary>
    /// Gets or sets the video tags.
    /// </summary>
    [JsonPropertyName("tags")]
    public List<string>? Tags { get; set; }

    /// <summary>
    /// Gets or sets the file extension.
    /// </summary>
    [JsonPropertyName("ext")]
    public string? Extension { get; set; }

    /// <summary>
    /// Gets or sets the webpage URL.
    /// </summary>
    [JsonPropertyName("webpage_url")]
    public string? WebpageUrl { get; set; }

    /// <summary>
    /// Gets or sets the extractor name.
    /// </summary>
    [JsonPropertyName("extractor")]
    public string? Extractor { get; set; }

    /// <summary>
    /// Gets the parsed upload date.
    /// </summary>
    [JsonIgnore]
    public DateTime? ParsedUploadDate
    {
        get
        {
            if (string.IsNullOrEmpty(UploadDate) || UploadDate.Length != 8)
            {
                return null;
            }

            if (int.TryParse(UploadDate[..4], out var year) &&
                int.TryParse(UploadDate[4..6], out var month) &&
                int.TryParse(UploadDate[6..8], out var day))
            {
                return new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Utc);
            }

            return null;
        }
    }
}
