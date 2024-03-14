using TheCardEditor.DataModel.DataModel;
using TheCardEditor.Shared.DTO;

namespace TheCardEditor.Services;

public class CardService
{
    private readonly DataContext _dataContext;

    public CardService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public void DeleteCard(long id)
    {
        _dataContext.Cards.Remove(_dataContext.Cards.First(f => f.Id == id));
        _dataContext.SaveChanges();
    }

    public void UpdateCard(CardModel model)
    {
        var cardSet = _dataContext.Cards.FirstOrDefault(f => f.Id == model.Id);
        if (cardSet == null)
        {
            _dataContext.Cards.Add(model.GetDataModel());
        }
        else
        {
            cardSet.Name = model.Name;
            cardSet.Data = model.Data;
        }
        _dataContext.SaveChanges();
    }

    public CardModel GetCard(long? cardId)
    {
        var card = _dataContext.Cards.FirstOrDefault(cs => cs.Id == cardId);
        return card == null ? new CardModel() : new CardModel(card);
    }

    public IEnumerable<CardModel> GetCards(long cardSetFk)
    {
        return _dataContext.Cards.Where(cs => cs.CardSetFk == cardSetFk).Select(g => new CardModel(g));
    }
}
