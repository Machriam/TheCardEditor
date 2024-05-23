using Microsoft.EntityFrameworkCore;
using TheCardEditor.DataModel.DataModel;
using TheCardEditor.Shared.DTO;

namespace TheCardEditor.Services;

public class GameService(DataContext dataContext)
{
    public void DeleteGame(GameModel model)
    {
        dataContext.Games.Remove(dataContext.Games.First(f => f.Id == model.Id));
        dataContext.SaveChanges();
    }

    public void UpdateGame(GameModel model)
    {
        var game = dataContext.Games.FirstOrDefault(f => f.Id == model.Id);
        if (game == null) dataContext.Games.Add(model.GetDataModel());
        else game.Name = model.Name;
        dataContext.SaveChanges();
    }

    public IEnumerable<GameSet> AllCardSets()
    {
        return dataContext.Games
            .Include(g => g.CardSets)
            .ToList()
            .SelectMany(g => g.CardSets.Select(cs => new GameSet(new CardSetModel(cs), new GameModel(g))));
    }

    public IEnumerable<GameModel> GetGames()
    {
        return dataContext.Games
            .Select(g => new GameModel(g));
    }
}
