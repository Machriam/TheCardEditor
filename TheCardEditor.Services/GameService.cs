using TheCardEditor.DataModel.DataModel;
using TheCardEditor.DataModel.DTO;

namespace TheCardEditor.Services;

public class CardSetService
{
    private readonly DataContext _dataContext;

    public CardSetService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public void DeleteCardSet(CardSetModel model)
    {
        _dataContext.CardSets.Remove(_dataContext.CardSets.First(f => f.Id == model.Id));
        _dataContext.SaveChanges();
    }

    public void UpdateCardSet(CardSetModel model)
    {
        var cardSet = _dataContext.CardSets.FirstOrDefault(f => f.Id == model.Id);
        if (cardSet == null) _dataContext.CardSets.Add(model.GetDataModel());
        else cardSet.Name = model.Name;
        _dataContext.SaveChanges();
    }

    public IEnumerable<CardSetModel> GetCardSets()
    {
        return _dataContext.CardSets.Select(g => new CardSetModel(g));
    }
}

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

    public void UpdateName(GameModel model)
    {
        var game = _dataContext.Games.FirstOrDefault(f => f.Id == model.Id);
        if (game == null) _dataContext.Games.Add(model.GetDataModel());
        else game.Name = model.Name;
        _dataContext.SaveChanges();
    }

    public IEnumerable<GameModel> GetFonts()
    {
        return _dataContext.Games.Select(g => new GameModel(g));
    }
}
