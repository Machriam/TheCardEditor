namespace TheCardEditor.DataModel.DataModel;

public partial class Game
{
    public Game()
    {
    }

    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<CardSet> CardSets { get; init; } = new List<CardSet>();
}
