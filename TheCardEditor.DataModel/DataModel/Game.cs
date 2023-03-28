namespace TheCardEditor.DataModel.DataModel;

public partial class Game
{
    public Game()
    {
    }

    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<CardSet> CardSets { get; init; } = new List<CardSet>();
}
