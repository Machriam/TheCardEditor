namespace TheCardEditor.DataModel.DataModel;

public partial class Card
{
    public Card()
    {
    }

    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int CardSetFk { get; set; }
    public string Data { get; set; } = null!;
    public virtual CardSet CardSetFkNavigation { get; set; } = null!;
}
