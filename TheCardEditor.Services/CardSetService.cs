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
        else
        {
            cardSet.Name = model.Name;
            cardSet.Zoom = model.Zoom;
        }
        _dataContext.SaveChanges();
    }

    public IEnumerable<CardSetModel> GetCardSets(long gameId)
    {
        return _dataContext.CardSets.Where(cs => cs.GameFk == gameId).Select(g => new CardSetModel(g));
    }
}
