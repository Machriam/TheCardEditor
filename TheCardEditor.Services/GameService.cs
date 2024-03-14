using TheCardEditor.DataModel.DataModel;
using TheCardEditor.Shared.DTO;

namespace TheCardEditor.Services;

public class GameService
{
    private readonly DataContext _dataContext;

    public GameService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public void DeleteGame(GameModel model)
    {
        _dataContext.Games.Remove(_dataContext.Games.First(f => f.Id == model.Id));
        _dataContext.SaveChanges();
    }

    public void UpdateGame(GameModel model)
    {
        var game = _dataContext.Games.FirstOrDefault(f => f.Id == model.Id);
        if (game == null) _dataContext.Games.Add(model.GetDataModel());
        else game.Name = model.Name;
        _dataContext.SaveChanges();
    }

    public IEnumerable<GameModel> GetGames()
    {
        return _dataContext.Games.Select(g => new GameModel(g));
    }
}
