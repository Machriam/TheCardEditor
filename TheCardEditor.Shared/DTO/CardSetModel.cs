using System.ComponentModel.DataAnnotations;
using TheCardEditor.DataModel.DataModel;

namespace TheCardEditor.DataModel.DTO;

public class CardSetModel
{
    public CardSetModel()
    {
    }

    public CardSetModel(CardSet cardSet)
    {
        Id = cardSet.Id;
        Name = cardSet.Name;
        Height = cardSet.Height;
        Width = cardSet.Width;
        GameFk = cardSet.GameFk;
    }

    public CardSet GetDataModel()
    {
        return new CardSet()
        {
            Name = Name,
            Width = Width,
            Height = Height,
            GameFk = GameFk,
            Zoom = Zoom
        };
    }

    public int Id { get; set; }

    [MinLength(3)]
    public string Name { get; set; } = null!;

    [Range(1, int.MaxValue)]
    public int Height { get; set; }

    [Range(1, int.MaxValue)]
    public int Width { get; set; }

    [Range(1, int.MaxValue)]
    public int GameFk { get; set; }

    [Range(1d, 500d)]
    public double Zoom { get; set; }
}
