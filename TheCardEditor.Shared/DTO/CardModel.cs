using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Nodes;
using TheCardEditor.DataModel.DataModel;

namespace TheCardEditor.Shared.DTO;

public class CardModel
{
    public static CardModel WithoutData(Card card)
    {
        return new CardModel()
        {
            Id = card.Id,
            Name = card.Name,
            CardSetFk = card.CardSetFk,
        };
    }

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

    public int Id { get; set; }

    [MinLength(3, ErrorMessage = "Please give the card a name with a length of atleast 3")]
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

    [Range(1, int.MaxValue)]
    public int CardSetFk { get; set; }
}
