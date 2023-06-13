using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using TheCardEditor.DataModel.DataModel;

namespace TheCardEditor.DataModel.DTO;

public class CardModel
{
    public CardModel()
    {
    }

    public CardModel(Card card)
    {
        Id = card.Id;
        Name = card.Name;
        Data = card.Data;
        CardSetFk = card.CardSetFk;
    }

    public Card GetDataModel()
    {
        return new Card()
        {
            Name = Name,
            CardSetFk = CardSetFk,
            Data = Data,
            Id = Id
        };
    }

    public long Id { get; set; }

    [MinLength(3)]
    public string Name { get; set; } = "";

    public List<string> GetTags()
    {
        return JsonSerializer.Deserialize<JsonObject>(Data)?["objects"]?.AsArray().Select(s => s["tag"]?.ToString() ?? "").ToList() ?? new();
    }
    public string Data { get; set; } = "{}";

    [Range(1, long.MaxValue)]
    public long CardSetFk { get; set; }
}
