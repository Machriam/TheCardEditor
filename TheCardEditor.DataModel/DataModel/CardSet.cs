namespace TheCardEditor.DataModel.DataModel;

public partial class CardSet
{
    public CardSet()
    {
    }

    public int Id { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public string Name { get; set; } = null!;
    public int GameFk { get; set; }
    public int Zoom { get; set; }
    public virtual ICollection<Card> Cards { get; init; } = new List<Card>();
    public virtual Game GameFkNavigation { get; set; } = null!;
    public virtual ICollection<Template> Templates { get; init; } = new List<Template>();
}
