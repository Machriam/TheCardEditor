using System.Text.Json;
using System.Text.Json.Nodes;

namespace TheCardEditor.Shared.Features.CardEditor;

public record struct Style(int Line, int Position, JsonNode? StyleNode);

public interface IStyleMerger
{
    IEnumerable<Style> ExtractStyles(JsonNode oldStyle);

    JsonNode? GetMergedStyle(IEnumerable<Style> styles, string[] inputLines, string[] outputLines);
}

public class StyleMerger() : IStyleMerger
{
    public IEnumerable<Style> ExtractStyles(JsonNode oldStyle)
    {
        return oldStyle.AsObject().ToDictionary()
            .SelectMany(a =>
            {
                var line = int.Parse(a.Key);
                return a.Value?.AsObject().ToDictionary()
                    .Select(c => new Style(line, int.Parse(c.Key), c.Value)) ?? [];
            });
    }

    public JsonNode? GetMergedStyle(IEnumerable<Style> styles, string[] inputLines, string[] outputLines)
    {
        var styleToUse = styles
            .FirstOrDefault(s => s.StyleNode != null).StyleNode?
            .AsObject()
            .ToDictionary(o => o.Key, o => (object?)(o.Value?.GetValueKind() == JsonValueKind.String ?
                o.Value.GetValue<string>() : o.Value?.GetValue<int>())) ?? new();
        var newStyle = outputLines.WithIndex().ToDictionary(l => l.Index.ToString(),
            l => l.Item.WithIndex().ToDictionary(c => c.Index.ToString(), _ => styleToUse));
        return JsonNode.Parse(newStyle.AsJson());
    }
}
