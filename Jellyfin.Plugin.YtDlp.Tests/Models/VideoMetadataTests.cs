using System.Text.Json;
using FluentAssertions;
using Jellyfin.Plugin.YtDlp.Models;

namespace Jellyfin.Plugin.YtDlp.Tests.Models;

public class VideoMetadataTests
{
    [Fact]
    public void ParsedUploadDate_WithValidDate_ReturnsDateTime()
    {
        var video = new VideoMetadata
        {
            Id = "test",
            Title = "Test",
            UploadDate = "20231225"
        };

        video.ParsedUploadDate.Should().Be(new DateTime(2023, 12, 25));
    }

    [Fact]
    public void ParsedUploadDate_WithNullDate_ReturnsNull()
    {
        var video = new VideoMetadata
        {
            Id = "test",
            Title = "Test",
            UploadDate = null
        };

        video.ParsedUploadDate.Should().BeNull();
    }

    [Fact]
    public void ParsedUploadDate_WithInvalidDate_ReturnsNull()
    {
        var video = new VideoMetadata
        {
            Id = "test",
            Title = "Test",
            UploadDate = "not-a-date"
        };

        video.ParsedUploadDate.Should().BeNull();
    }

    [Fact]
    public void ParsedUploadDate_WithShortDate_ReturnsNull()
    {
        var video = new VideoMetadata
        {
            Id = "test",
            Title = "Test",
            UploadDate = "202312"
        };

        video.ParsedUploadDate.Should().BeNull();
    }

    [Fact]
    public void Deserialize_FromYtDlpJson_ParsesCorrectly()
    {
        var json = """
        {
            "id": "dQw4w9WgXcQ",
            "title": "Rick Astley - Never Gonna Give You Up",
            "description": "The official video",
            "channel": "Rick Astley",
            "channel_id": "UCuAXFkgsw1L7xaCfnd5JJOw",
            "uploader": "Rick Astley",
            "uploader_id": "@RickAstleyYT",
            "upload_date": "20091025",
            "duration": 213,
            "view_count": 1500000000,
            "like_count": 15000000,
            "tags": ["Rick Astley", "Never Gonna Give You Up", "80s"],
            "categories": ["Music"]
        }
        """;

        var video = JsonSerializer.Deserialize<VideoMetadata>(json);

        video.Should().NotBeNull();
        video!.Id.Should().Be("dQw4w9WgXcQ");
        video.Title.Should().Be("Rick Astley - Never Gonna Give You Up");
        video.Description.Should().Be("The official video");
        video.Channel.Should().Be("Rick Astley");
        video.ChannelId.Should().Be("UCuAXFkgsw1L7xaCfnd5JJOw");
        video.Uploader.Should().Be("Rick Astley");
        video.UploaderId.Should().Be("@RickAstleyYT");
        video.UploadDate.Should().Be("20091025");
        video.Duration.Should().Be(213);
        video.ViewCount.Should().Be(1500000000);
        video.LikeCount.Should().Be(15000000);
        video.Tags.Should().BeEquivalentTo(["Rick Astley", "Never Gonna Give You Up", "80s"]);
        video.Categories.Should().BeEquivalentTo(["Music"]);
        video.ParsedUploadDate.Should().Be(new DateTime(2009, 10, 25));
    }

    [Fact]
    public void Deserialize_WithMissingOptionalFields_SetsNulls()
    {
        var json = """
        {
            "id": "test123",
            "title": "Test Video"
        }
        """;

        var video = JsonSerializer.Deserialize<VideoMetadata>(json);

        video.Should().NotBeNull();
        video!.Description.Should().BeNull();
        video.Channel.Should().BeNull();
        video.Tags.Should().BeNull();
        video.Duration.Should().BeNull();
    }

    [Fact]
    public void Deserialize_WithEmptyTags_ParsesAsEmptyArray()
    {
        var json = """
        {
            "id": "test123",
            "title": "Test Video",
            "tags": []
        }
        """;

        var video = JsonSerializer.Deserialize<VideoMetadata>(json);

        video.Should().NotBeNull();
        video!.Tags.Should().BeEmpty();
    }
}
