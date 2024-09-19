using FluentAssertions;
using NSubstitute;
using System;
using System.Text.Json.Nodes;
using TheCardEditor.Shared.Features.CardEditor;
using Xunit;

namespace TheCardEditor.Shared.Tests.Features.CardEditor;

public class StyleMergerTests
{
    private static string s_currentPath = Path.Combine(Directory.GetCurrentDirectory().GetApplicationRootGitPath() ?? "",
        "TheCardEditor.Tests", "Features", "CardEditor");

    public StyleMergerTests()
    {
    }

    private StyleMerger CreateStyleMerger()
    {
        return new StyleMerger();
    }

    [Fact]
    public void ExtractStyles_ShouldWork()
    {
        var sut = CreateStyleMerger();
        var extractStyle = File.ReadAllText(Path.Combine(s_currentPath, "ExtractStyleExample.json"));
        var expected = File.ReadAllText(Path.Combine(s_currentPath, "MergeStyleExample.json")).FromJson<IEnumerable<Style>>();
        var result = sut.ExtractStyles(JsonNode.Parse(extractStyle)!);
        result.AsJson().Should().BeEquivalentTo(expected.AsJson());
    }

    [Fact]
    public void GetMergedStyle_ShouldWork()
    {
        var sut = CreateStyleMerger();
        var style = File.ReadAllText(Path.Combine(s_currentPath, "MergeStyleExample.json")).FromJson<IEnumerable<Style>>() ?? [];
        var expectedStyle = JsonNode.Parse(File.ReadAllText(Path.Combine(s_currentPath, "MergeStyleExampleExpected.json")));
        var result = sut.GetMergedStyle(style, ["Athleticus, der Halbgott", "der Gewandtheit"], ["Athleticus, der Halbgott", "", "der Gewandtheit"]);
        expectedStyle.AsJson().Should().BeEquivalentTo(result.AsJson());
    }
}
