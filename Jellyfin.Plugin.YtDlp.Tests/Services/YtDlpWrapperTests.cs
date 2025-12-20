using FluentAssertions;
using Jellyfin.Plugin.YtDlp.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Jellyfin.Plugin.YtDlp.Tests.Services;

public class YtDlpWrapperTests : IDisposable
{
    private readonly Mock<IYtDlpBinaryManager> _binaryManagerMock;
    private readonly Mock<ILogger<YtDlpWrapper>> _loggerMock;
    private readonly YtDlpWrapper _sut;
    private readonly string _tempDir;

    public YtDlpWrapperTests()
    {
        _binaryManagerMock = new Mock<IYtDlpBinaryManager>();
        _loggerMock = new Mock<ILogger<YtDlpWrapper>>();
        _sut = new YtDlpWrapper(_binaryManagerMock.Object, _loggerMock.Object);

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
    public void IsInArchive_WhenFileDoesNotExist_ReturnsFalse()
    {
        var archivePath = Path.Combine(_tempDir, "nonexistent.txt");

        var result = _sut.IsInArchive(archivePath, "test123");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInArchive_WhenVideoNotInArchive_ReturnsFalse()
    {
        var archivePath = Path.Combine(_tempDir, "archive.txt");
        File.WriteAllText(archivePath, "youtube abc123\nyoutube def456\n");

        var result = _sut.IsInArchive(archivePath, "xyz789");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInArchive_WhenVideoInArchive_ReturnsTrue()
    {
        var archivePath = Path.Combine(_tempDir, "archive.txt");
        File.WriteAllText(archivePath, "youtube abc123\nyoutube def456\n");

        var result = _sut.IsInArchive(archivePath, "abc123");

        result.Should().BeTrue();
    }

    [Fact]
    public void IsInArchive_IsCaseSensitive()
    {
        var archivePath = Path.Combine(_tempDir, "archive.txt");
        File.WriteAllText(archivePath, "youtube ABC123\n");

        var result = _sut.IsInArchive(archivePath, "abc123");

        result.Should().BeFalse();
    }

    [Fact]
    public void IsInArchive_DoesNotMatchPartialId()
    {
        var archivePath = Path.Combine(_tempDir, "archive.txt");
        File.WriteAllText(archivePath, "youtube abc123456\n");

        var result = _sut.IsInArchive(archivePath, "abc123");

        result.Should().BeFalse();
    }

    [Fact]
    public void AddToArchive_CreatesFileIfNotExists()
    {
        var archivePath = Path.Combine(_tempDir, "newarchive.txt");

        _sut.AddToArchive(archivePath, "test123");

        File.Exists(archivePath).Should().BeTrue();
    }

    [Fact]
    public void AddToArchive_AppendsToExistingFile()
    {
        var archivePath = Path.Combine(_tempDir, "archive.txt");
        File.WriteAllText(archivePath, "youtube abc123\n");

        _sut.AddToArchive(archivePath, "def456");

        var content = File.ReadAllText(archivePath);
        content.Should().Contain("youtube abc123");
        content.Should().Contain("youtube def456");
    }

    [Fact]
    public void AddToArchive_WritesCorrectFormat()
    {
        var archivePath = Path.Combine(_tempDir, "archive.txt");

        _sut.AddToArchive(archivePath, "dQw4w9WgXcQ");

        var content = File.ReadAllText(archivePath);
        content.Should().Contain("youtube dQw4w9WgXcQ");
    }

    [Fact]
    public void AddToArchive_CreatesDirectoryIfNeeded()
    {
        var archivePath = Path.Combine(_tempDir, "subdir", "archive.txt");

        _sut.AddToArchive(archivePath, "test123");

        File.Exists(archivePath).Should().BeTrue();
    }

    [Fact]
    public void AddToArchive_MakesVideoFindableByIsInArchive()
    {
        var archivePath = Path.Combine(_tempDir, "archive.txt");

        _sut.AddToArchive(archivePath, "test123");

        _sut.IsInArchive(archivePath, "test123").Should().BeTrue();
    }
}
