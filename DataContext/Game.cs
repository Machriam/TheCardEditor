namespace TheCardEditor.DataContext;

public partial class Game
{
    public Game()
    {
    }

    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public virtual ICollection<Card> Cards { get; init; } = new List<Card>();
}
