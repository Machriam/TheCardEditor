namespace TheCardEditor.DataModel.DataModel;

public partial class CardSet
{
    public CardSet()
    {
    }

    public long Id { get; set; }
    public long Height { get; set; }
    public long Width { get; set; }
    public string Name { get; set; } = null!;
    public long GameFk { get; set; }
    public virtual ICollection<Card> Cards { get; init; } = new List<Card>();
    public virtual Game GameFkNavigation { get; set; } = null!;
}
