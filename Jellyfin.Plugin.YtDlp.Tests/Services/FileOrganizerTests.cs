using FluentAssertions;
using Jellyfin.Plugin.YtDlp.Services;
using Moq;

namespace Jellyfin.Plugin.YtDlp.Tests.Services;

public class FileOrganizerTests
{
    private readonly Mock<IRuleEngine> _ruleEngineMock;
    private readonly FileOrganizer _sut;

    public FileOrganizerTests()
    {
        _ruleEngineMock = new Mock<IRuleEngine>();
        _sut = new FileOrganizer(_ruleEngineMock.Object);
    }

    [Fact]
    public void SanitizeName_WithNormalName_ReturnsUnchanged()
    {
        var result = _sut.SanitizeName("Normal Name");

        result.Should().Be("Normal Name");
    }

    [Fact]
    public void SanitizeName_RemovesSlashes()
    {
        // Slashes are invalid on all platforms.
        var result = _sut.SanitizeName("Name/With/Slashes");

        result.Should().NotContain("/");
    }

    [Fact]
    public void SanitizeName_RemovesNullCharacters()
    {
        var result = _sut.SanitizeName("Name\0With\0Nulls");

        result.Should().NotContain("\0");
    }

    [Theory]
    [InlineData("  Leading spaces", "Leading spaces")]
    [InlineData("Trailing spaces  ", "Trailing spaces")]
    [InlineData("  Both ends  ", "Both ends")]
    public void SanitizeName_TrimsWhitespace(string input, string expected)
    {
        var result = _sut.SanitizeName(input);

        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("Multiple   Spaces", "Multiple Spaces")]
    [InlineData("Many     Spaces", "Many Spaces")]
    public void SanitizeName_CollapsesMultipleSpaces(string input, string expected)
    {
        var result = _sut.SanitizeName(input);

        result.Should().Be(expected);
    }

    [Fact]
    public void SanitizeName_TruncatesLongNames()
    {
        var longName = new string('A', 150);

        var result = _sut.SanitizeName(longName);

        result.Should().HaveLength(100);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SanitizeName_ReturnsUnknownForEmptyOrWhitespace(string input)
    {
        var result = _sut.SanitizeName(input);

        result.Should().Be("Unknown");
    }

    [Fact]
    public void SanitizeName_ReturnsUnknownWhenAllCharsInvalid()
    {
        var result = _sut.SanitizeName("/\0/\0");

        result.Should().Be("Unknown");
    }

    [Fact]
    public void SanitizeName_PreservesValidSpecialCharacters()
    {
        var result = _sut.SanitizeName("Name - With (Parentheses) & Ampersand");

        result.Should().Be("Name - With (Parentheses) & Ampersand");
    }

    [Fact]
    public void SanitizeName_PreservesUnicodeCharacters()
    {
        var result = _sut.SanitizeName("日本語チャンネル");

        result.Should().Be("日本語チャンネル");
    }

    [Fact]
    public void SanitizeName_PreservesEmoji()
    {
        var result = _sut.SanitizeName("Channel 🎮 Gaming");

        result.Should().Be("Channel 🎮 Gaming");
    }
}
