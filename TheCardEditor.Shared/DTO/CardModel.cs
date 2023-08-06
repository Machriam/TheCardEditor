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

    public JsonObject SerializedData()
    {
        return JsonSerializer.Deserialize<JsonObject>(Data) ?? new();
    }

    public JsonObject VirtualSerializedData()
    {
        return JsonSerializer.Deserialize<JsonObject>(VirtualData) ?? new();
    }

    private string _data = "{}";

    public string Data
    {
        get => _data; set
        {
            _data = value;
            VirtualData = _data;
        }
    }

    public string VirtualData { get; set; } = "{}";

    public bool IsModified()
    {
        return Data != VirtualData;
    }

    [Range(1, long.MaxValue)]
    public long CardSetFk { get; set; }
}
