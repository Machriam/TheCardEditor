using System.Text.Json.Nodes;

namespace TheCardEditor.Shared;

public static class JsonObjectExtensions
{
    public static IEnumerable<(string Tag, string Text)> GetTags(this JsonObject json)
    {
        return json?["objects"]?.AsArray()
                                .Select(s => (s?["tag"]?.ToString() ?? "", s?["text"]?.ToString() ?? "")) ?? Array.Empty<(string, string)>();
    }
}
