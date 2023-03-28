namespace TheCardEditor.DataModel.DataModel;

public partial class Card
{
    public Card()
    {
    }

    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long? CardSetFk { get; set; }
    public string Data { get; set; } = null!;
    public virtual CardSet? CardSetFkNavigation { get; set; }
}
