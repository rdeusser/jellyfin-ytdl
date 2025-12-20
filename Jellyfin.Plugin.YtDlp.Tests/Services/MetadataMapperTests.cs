using System.Xml.Linq;
using FluentAssertions;
using Jellyfin.Plugin.YtDlp.Models;
using Jellyfin.Plugin.YtDlp.Services;

namespace Jellyfin.Plugin.YtDlp.Tests.Services;

public class MetadataMapperTests : IDisposable
{
    private readonly MetadataMapper _sut = new();
    private readonly string _tempDir;

    public MetadataMapperTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"ytdlp-tests-{Guid.NewGuid()}");
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, recursive: true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task WriteNfoFileAsync_CreatesNfoFile()
    {
        var video = CreateVideo();
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        File.Exists(nfoPath).Should().BeTrue();
    }

    [Fact]
    public async Task WriteNfoFileAsync_WritesTitle()
    {
        var video = CreateVideo(title: "My Awesome Video");
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var doc = XDocument.Load(nfoPath);
        doc.Root!.Element("title")!.Value.Should().Be("My Awesome Video");
    }

    [Fact]
    public async Task WriteNfoFileAsync_WritesDescription()
    {
        var video = CreateVideo(description: "This is a test description");
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var doc = XDocument.Load(nfoPath);
        doc.Root!.Element("plot")!.Value.Should().Be("This is a test description");
    }

    [Fact]
    public async Task WriteNfoFileAsync_OmitsPlotWhenDescriptionNull()
    {
        var video = CreateVideo(description: null);
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var doc = XDocument.Load(nfoPath);
        doc.Root!.Element("plot").Should().BeNull();
    }

    [Fact]
    public async Task WriteNfoFileAsync_WritesUploadDate()
    {
        var video = CreateVideo();
        video.UploadDate = "20231225";
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var doc = XDocument.Load(nfoPath);
        doc.Root!.Element("aired")!.Value.Should().Be("2023-12-25");
    }

    [Fact]
    public async Task WriteNfoFileAsync_WritesDuration()
    {
        var video = CreateVideo();
        video.Duration = 3600; // 60 minutes
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var doc = XDocument.Load(nfoPath);
        doc.Root!.Element("runtime")!.Value.Should().Be("60");
    }

    [Fact]
    public async Task WriteNfoFileAsync_WritesChannel()
    {
        var video = CreateVideo();
        video.Channel = "My Channel";
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var doc = XDocument.Load(nfoPath);
        doc.Root!.Element("studio")!.Value.Should().Be("My Channel");
    }

    [Fact]
    public async Task WriteNfoFileAsync_WritesTags()
    {
        var video = CreateVideo();
        video.Tags = ["tag1", "tag2", "tag3"];
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var doc = XDocument.Load(nfoPath);
        var tags = doc.Root!.Elements("tag").Select(e => e.Value).ToList();
        tags.Should().BeEquivalentTo(["tag1", "tag2", "tag3"]);
    }

    [Fact]
    public async Task WriteNfoFileAsync_WritesCategories()
    {
        var video = CreateVideo();
        video.Categories = ["Gaming", "Entertainment"];
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var doc = XDocument.Load(nfoPath);
        var genres = doc.Root!.Elements("genre").Select(e => e.Value).ToList();
        genres.Should().BeEquivalentTo(["Gaming", "Entertainment"]);
    }

    [Fact]
    public async Task WriteNfoFileAsync_WritesYouTubeId()
    {
        var video = CreateVideo();
        video.Id = "dQw4w9WgXcQ";
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var doc = XDocument.Load(nfoPath);
        var uniqueId = doc.Root!.Element("uniqueid");
        uniqueId!.Value.Should().Be("dQw4w9WgXcQ");
        uniqueId.Attribute("type")!.Value.Should().Be("youtube");
        uniqueId.Attribute("default")!.Value.Should().Be("true");
    }

    [Fact]
    public async Task WriteNfoFileAsync_CreatesValidXml()
    {
        var video = CreateVideo(
            title: "Test <Video> & \"Special\" 'Chars'",
            description: "Description with <html> & stuff");
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        await _sut.WriteNfoFileAsync(video, videoPath);

        var nfoPath = Path.Combine(_tempDir, "video.nfo");
        var act = () => XDocument.Load(nfoPath);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task WriteShowNfoFileAsync_CreatesNfoFile()
    {
        var channelPath = Path.Combine(_tempDir, "channel");

        await _sut.WriteShowNfoFileAsync("My Channel", "UC123", channelPath);

        var nfoPath = Path.Combine(channelPath, "tvshow.nfo");
        File.Exists(nfoPath).Should().BeTrue();
    }

    [Fact]
    public async Task WriteShowNfoFileAsync_CreatesDirectory()
    {
        var channelPath = Path.Combine(_tempDir, "new-channel");

        await _sut.WriteShowNfoFileAsync("My Channel", "UC123", channelPath);

        Directory.Exists(channelPath).Should().BeTrue();
    }

    [Fact]
    public async Task WriteShowNfoFileAsync_WritesChannelName()
    {
        var channelPath = Path.Combine(_tempDir, "channel");

        await _sut.WriteShowNfoFileAsync("My Awesome Channel", "UC123", channelPath);

        var nfoPath = Path.Combine(channelPath, "tvshow.nfo");
        var doc = XDocument.Load(nfoPath);
        doc.Root!.Element("title")!.Value.Should().Be("My Awesome Channel");
    }

    [Fact]
    public async Task WriteShowNfoFileAsync_WritesChannelId()
    {
        var channelPath = Path.Combine(_tempDir, "channel");

        await _sut.WriteShowNfoFileAsync("My Channel", "UC123ABC", channelPath);

        var nfoPath = Path.Combine(channelPath, "tvshow.nfo");
        var doc = XDocument.Load(nfoPath);
        var uniqueId = doc.Root!.Element("uniqueid");
        uniqueId!.Value.Should().Be("UC123ABC");
        uniqueId.Attribute("type")!.Value.Should().Be("youtube");
    }

    [Fact]
    public async Task WriteShowNfoFileAsync_DoesNotOverwriteExisting()
    {
        var channelPath = Path.Combine(_tempDir, "channel");
        Directory.CreateDirectory(channelPath);
        var nfoPath = Path.Combine(channelPath, "tvshow.nfo");
        await File.WriteAllTextAsync(nfoPath, "original content");

        await _sut.WriteShowNfoFileAsync("New Channel", "UC456", channelPath);

        var content = await File.ReadAllTextAsync(nfoPath);
        content.Should().Be("original content");
    }

    [Fact]
    public async Task WriteNfoFileAsync_HandlesNullTags()
    {
        var video = CreateVideo();
        video.Tags = null;
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        var act = async () => await _sut.WriteNfoFileAsync(video, videoPath);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task WriteNfoFileAsync_HandlesNullCategories()
    {
        var video = CreateVideo();
        video.Categories = null;
        var videoPath = Path.Combine(_tempDir, "video.mp4");

        var act = async () => await _sut.WriteNfoFileAsync(video, videoPath);

        await act.Should().NotThrowAsync();
    }

    private static VideoMetadata CreateVideo(
        string title = "Test Video",
        string? description = "Test description")
    {
        return new VideoMetadata
        {
            Id = "test-id",
            Title = title,
            Description = description
        };
    }
}
