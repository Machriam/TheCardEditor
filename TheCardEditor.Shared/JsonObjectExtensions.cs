using System.Text.Json.Nodes;

namespace TheCardEditor.Shared;

public static class JsonObjectExtensions
{
    public static IEnumerable<string> GetTags(this JsonObject json)
    {
        return json?["objects"]?.AsArray()
                                .Select(s => s?["tag"]?.ToString() ?? "") ?? Array.Empty<string>();
    }
}
