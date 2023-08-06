using System.Text.Json.Nodes;

namespace TheCardEditor.Shared;

public static class JsonObjectExtensions
{
    public static IEnumerable<(string Tag, string Text)> GetTags(this JsonObject json)
    {
        return json?["objects"]?.AsArray()
                                .Select(s => (s?["tag"]?.ToString() ?? "", s?["text"]?.ToString() ?? ""))
                                .Where(s => !string.IsNullOrEmpty(s.Item1)) ?? Array.Empty<(string, string)>();
    }

    public static IEnumerable<long> GetPictureIds(this JsonObject json)
    {
        return json?["objects"]?.AsArray()
                                .Select(s => (s?["type"]?.ToString() ?? "", s?["pictureId"]?.ToString() ?? ""))
                                .Where(s => s.Item1 == "image")
                                .Select(s => long.Parse(s.Item2)) ?? new List<long>();
    }
}
