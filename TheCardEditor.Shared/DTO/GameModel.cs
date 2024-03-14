using TheCardEditor.DataModel.DataModel;

namespace TheCardEditor.Shared.DTO;

public class GameModel
{
    public GameModel()
    {
    }

    public GameModel(Game game)
    {
        Id = game.Id;
        Name = game.Name;
    }

    public Game GetDataModel()
    {
        return new Game()
        {
            Name = Name,
        };
    }

    public int Id { get; set; }
    public string Name { get; set; } = null!;
}
