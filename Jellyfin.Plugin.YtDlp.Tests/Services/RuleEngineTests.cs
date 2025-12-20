using FluentAssertions;
using Jellyfin.Plugin.YtDlp.Models;
using Jellyfin.Plugin.YtDlp.Services;

namespace Jellyfin.Plugin.YtDlp.Tests.Services;

public class RuleEngineTests
{
    private readonly RuleEngine _sut = new();

    [Fact]
    public void EvaluateRules_WithNoRules_ReturnsNull()
    {
        var video = CreateVideo("Test Video");
        var rules = Array.Empty<Rule>();

        var result = _sut.EvaluateRules(video, rules);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateRules_WithNoConditions_MatchesAll()
    {
        // Rules with no conditions match all videos (vacuously true).
        var video = CreateVideo("Test Video");
        var rules = new[]
        {
            new Rule { Name = "Empty Rule", FolderName = "Folder", Conditions = [] }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Folder");
    }

    [Fact]
    public void EvaluateRules_TitleContains_MatchesCorrectly()
    {
        var video = CreateVideo("Let's Play Minecraft Episode 1");
        var rules = new[]
        {
            new Rule
            {
                Name = "Gaming",
                FolderName = "Gaming",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Minecraft" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Gaming");
    }

    [Fact]
    public void EvaluateRules_TitleContains_CaseInsensitive()
    {
        var video = CreateVideo("Let's Play MINECRAFT Episode 1");
        var rules = new[]
        {
            new Rule
            {
                Name = "Gaming",
                FolderName = "Gaming",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "minecraft" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Gaming");
    }

    [Fact]
    public void EvaluateRules_TitleStartsWith_MatchesCorrectly()
    {
        var video = CreateVideo("[Tutorial] How to Code");
        var rules = new[]
        {
            new Rule
            {
                Name = "Tutorials",
                FolderName = "Tutorials",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.StartsWith, Value = "[Tutorial]" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Tutorials");
    }

    [Fact]
    public void EvaluateRules_TitleEndsWith_MatchesCorrectly()
    {
        var video = CreateVideo("My Video - Part 1");
        var rules = new[]
        {
            new Rule
            {
                Name = "Series",
                FolderName = "Series",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.EndsWith, Value = "Part 1" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Series");
    }

    [Fact]
    public void EvaluateRules_TitleEquals_MatchesExactly()
    {
        var video = CreateVideo("Exact Title");
        var rules = new[]
        {
            new Rule
            {
                Name = "Exact",
                FolderName = "Exact",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Equals, Value = "Exact Title" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Exact");
    }

    [Fact]
    public void EvaluateRules_TitleEquals_DoesNotMatchPartial()
    {
        var video = CreateVideo("Exact Title With More");
        var rules = new[]
        {
            new Rule
            {
                Name = "Exact",
                FolderName = "Exact",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Equals, Value = "Exact Title" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateRules_TitleNotContains_MatchesCorrectly()
    {
        var video = CreateVideo("My Regular Video");
        var rules = new[]
        {
            new Rule
            {
                Name = "NotSponsored",
                FolderName = "Regular",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.NotContains, Value = "sponsored" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Regular");
    }

    [Fact]
    public void EvaluateRules_DescriptionContains_MatchesCorrectly()
    {
        var video = CreateVideo("My Video", description: "This video contains sponsored content");
        var rules = new[]
        {
            new Rule
            {
                Name = "Sponsored",
                FolderName = "Sponsored",
                Conditions = [new Condition { Field = ConditionField.Description, Operator = ConditionOperator.Contains, Value = "sponsored" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Sponsored");
    }

    [Fact]
    public void EvaluateRules_TagsContains_MatchesAnyTag()
    {
        var video = CreateVideo("Gaming Video", tags: new List<string> { "gaming", "minecraft", "tutorial" });
        var rules = new[]
        {
            new Rule
            {
                Name = "Minecraft",
                FolderName = "Minecraft",
                Conditions = [new Condition { Field = ConditionField.Tags, Operator = ConditionOperator.Contains, Value = "minecraft" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Minecraft");
    }

    [Fact]
    public void EvaluateRules_CategoryContains_MatchesAnyCategory()
    {
        var video = CreateVideo("Music Video", categories: new List<string> { "Music", "Entertainment" });
        var rules = new[]
        {
            new Rule
            {
                Name = "Music",
                FolderName = "Music",
                Conditions = [new Condition { Field = ConditionField.Category, Operator = ConditionOperator.Contains, Value = "Music" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Music");
    }

    [Fact]
    public void EvaluateRules_MultipleConditions_RequiresAllToMatch()
    {
        var video = CreateVideo("[Tutorial] Minecraft Building", tags: new List<string> { "minecraft", "tutorial" });
        var rules = new[]
        {
            new Rule
            {
                Name = "Minecraft Tutorials",
                FolderName = "MinecraftTutorials",
                Conditions =
                [
                    new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Minecraft" },
                    new Condition { Field = ConditionField.Title, Operator = ConditionOperator.StartsWith, Value = "[Tutorial]" }
                ]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("MinecraftTutorials");
    }

    [Fact]
    public void EvaluateRules_MultipleConditions_FailsIfOneDoesNotMatch()
    {
        var video = CreateVideo("Minecraft Building Guide", tags: new List<string> { "minecraft" });
        var rules = new[]
        {
            new Rule
            {
                Name = "Minecraft Tutorials",
                FolderName = "MinecraftTutorials",
                Conditions =
                [
                    new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Minecraft" },
                    new Condition { Field = ConditionField.Title, Operator = ConditionOperator.StartsWith, Value = "[Tutorial]" }
                ]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateRules_HigherPriorityRulesCheckedFirst()
    {
        var video = CreateVideo("Minecraft Tutorial");
        var rules = new[]
        {
            new Rule
            {
                Name = "General Gaming",
                FolderName = "Gaming",
                Priority = 1,
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Minecraft" }]
            },
            new Rule
            {
                Name = "Tutorials",
                FolderName = "Tutorials",
                Priority = 10,
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Tutorial" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Tutorials");
    }

    [Fact]
    public void EvaluateRules_NestedRules_ReturnsFullPath()
    {
        var video = CreateVideo("Minecraft Redstone Tutorial");
        var rules = new[]
        {
            new Rule
            {
                Name = "Gaming",
                FolderName = "Gaming",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Minecraft" }],
                Children =
                [
                    new Rule
                    {
                        Name = "Redstone",
                        FolderName = "Redstone",
                        Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Redstone" }]
                    }
                ]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Gaming/Redstone");
    }

    [Fact]
    public void EvaluateRules_NestedRules_StopsAtParentIfChildDoesNotMatch()
    {
        var video = CreateVideo("Minecraft Building Guide");
        var rules = new[]
        {
            new Rule
            {
                Name = "Gaming",
                FolderName = "Gaming",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Minecraft" }],
                Children =
                [
                    new Rule
                    {
                        Name = "Redstone",
                        FolderName = "Redstone",
                        Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Redstone" }]
                    }
                ]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Gaming");
    }

    [Fact]
    public void EvaluateRules_DeeplyNestedRules_ReturnsFullPath()
    {
        var video = CreateVideo("Advanced Minecraft Redstone Clock Tutorial");
        var rules = new[]
        {
            new Rule
            {
                Name = "Gaming",
                FolderName = "Gaming",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Minecraft" }],
                Children =
                [
                    new Rule
                    {
                        Name = "Redstone",
                        FolderName = "Redstone",
                        Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Redstone" }],
                        Children =
                        [
                            new Rule
                            {
                                Name = "Clocks",
                                FolderName = "Clocks",
                                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "Clock" }]
                            }
                        ]
                    }
                ]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Gaming/Redstone/Clocks");
    }

    [Fact]
    public void EvaluateRules_NullTitle_DoesNotThrow()
    {
        var video = new VideoMetadata { Id = "test", Title = null! };
        var rules = new[]
        {
            new Rule
            {
                Name = "Test",
                FolderName = "Test",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.Contains, Value = "test" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateRules_NullTags_DoesNotThrow()
    {
        var video = new VideoMetadata { Id = "test", Title = "Test", Tags = null };
        var rules = new[]
        {
            new Rule
            {
                Name = "Test",
                FolderName = "Test",
                Conditions = [new Condition { Field = ConditionField.Tags, Operator = ConditionOperator.Contains, Value = "test" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().BeNull();
    }

    [Fact]
    public void EvaluateRules_NotEquals_MatchesCorrectly()
    {
        var video = CreateVideo("My Video");
        var rules = new[]
        {
            new Rule
            {
                Name = "NotExact",
                FolderName = "Other",
                Conditions = [new Condition { Field = ConditionField.Title, Operator = ConditionOperator.NotEquals, Value = "Exact Title" }]
            }
        };

        var result = _sut.EvaluateRules(video, rules);

        result.Should().Be("Other");
    }

    private static VideoMetadata CreateVideo(
        string title,
        string? description = null,
        List<string>? tags = null,
        List<string>? categories = null)
    {
        return new VideoMetadata
        {
            Id = "test-id",
            Title = title,
            Description = description,
            Tags = tags,
            Categories = categories
        };
    }
}
