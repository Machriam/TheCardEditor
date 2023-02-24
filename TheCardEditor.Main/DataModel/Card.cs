namespace TheCardEditor.DataModel;

public partial class Card
{
    public Card()
    {
    }

    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public long Height { get; set; }
    public long Width { get; set; }
    public long GameFk { get; set; }
    public virtual Game GameFkNavigation { get; set; } = null!;
    public virtual ICollection<Layer> Layers { get; init; } = new List<Layer>();
}
