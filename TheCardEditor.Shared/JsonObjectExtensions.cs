using System.Text.Json.Nodes;
using TheCardEditor.Shared.Features.CardEditor;

namespace TheCardEditor.Shared;

public static class JsonObjectExtensions
{
    private const string TagKey = "tag";
    private const string ObjectsKey = "objects";
    private const string TextKey = "text";
    private const string StylesKey = "styles";
    private const string ImageKey = "image";
    private const string TypeKey = "type";
    private const string PictureIdKey = "pictureId";

    public static IEnumerable<(string Tag, string Text)> GetTags(this JsonObject json)
    {
        return json?[ObjectsKey]?.AsArray()
                                .Select(s => (s?[TagKey]?.ToString() ?? "", s?[TextKey]?.ToString() ?? ""))
                                .Where(s => !string.IsNullOrEmpty(s.Item1)) ?? [];
    }

    public static JsonObject UpdateTags(this JsonObject json, Dictionary<string, string> newTags, IStyleMerger merger)
    {
        foreach (var item in json[ObjectsKey]?.AsArray() ?? [])
        {
            if (item == null || item[TagKey] == null) continue;
            var oldTextLines = item[TextKey]?.GetValue<string>().Split("\n") ?? [];
            var styles = merger.ExtractStyles(item[StylesKey] ?? new JsonObject());
            item[TextKey] = newTags.TryGetValue(item[TagKey]?.ToString() ?? "", out var newTag) ? newTag : "";
            var newTextLines = newTag?.Split("\n") ?? [];
            item[StylesKey] = merger.GetMergedStyle(styles, oldTextLines, newTextLines);
        }
        return json;
    }

    public static IEnumerable<(string Tag, long PictureId, int Index)> GetObjects(this JsonObject json)
    {
        return json?[ObjectsKey]?.AsArray()
                                .Select((s, i) => (s?[TagKey]?.ToString() ?? "",
                                long.TryParse(s?["pictureId"]?.ToString(), out var pictureId) ? pictureId : -1, i))?
                                .ToList() ?? new();
    }

    public static IEnumerable<long> GetPictureIds(this JsonObject json)
    {
        return json?[ObjectsKey]?.AsArray()
                                .Select(s => (s?[TypeKey]?.ToString() ?? "", s?[PictureIdKey]?.ToString() ?? ""))
                                .Where(s => s.Item1 == ImageKey)
                                .Select(s => long.Parse(s.Item2)) ?? [];
    }
}
