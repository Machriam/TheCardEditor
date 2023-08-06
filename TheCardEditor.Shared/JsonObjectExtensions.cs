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

    public static IEnumerable<(string Tag, long PictureId, int Index)> GetObjects(this JsonObject json)
    {
        return json?["objects"]?.AsArray()
                                .Select((s, i) => (s?["tag"]?.ToString() ?? "",
                                long.TryParse(s?["pictureId"]?.ToString(), out var pictureId) ? pictureId : -1, i))?
                                .ToList() ?? new();
    }

    public static IEnumerable<long> GetPictureIds(this JsonObject json)
    {
        return json?["objects"]?.AsArray()
                                .Select(s => (s?["type"]?.ToString() ?? "", s?["pictureId"]?.ToString() ?? ""))
                                .Where(s => s.Item1 == "image")
                                .Select(s => long.Parse(s.Item2)) ?? new List<long>();
    }
}
