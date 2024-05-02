using TheCardEditor.DataModel.DataModel;
using TheCardEditor.Shared;
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
        _dataContext.PictureCardReferences.RemoveRange(
            _dataContext.PictureCardReferences.Where(r => r.CardFk == id));
        _dataContext.SaveChanges();
    }

    public void UpdateCard(CardModel model)
    {
        var cardSet = _dataContext.Cards.FirstOrDefault(f => f.Id == model.Id);
        var pictureIds = model.SerializedData().GetPictureIds();
        if (cardSet == null)
        {
            var card = model.GetDataModel();
            foreach (var id in pictureIds) card.PictureCardReferences.Add(new PictureCardReference() { PictureFk = (int)id });
            _dataContext.Cards.Add(card);
        }
        else
        {
            cardSet.Name = model.Name;
            cardSet.Data = model.Data;
            _dataContext.PictureCardReferences.RemoveRange(_dataContext.PictureCardReferences.Where(r => r.CardFk == model.Id));
            _dataContext.PictureCardReferences.AddRange(pictureIds.Select(p => new PictureCardReference()
            {
                PictureFk = (int)p,
                CardFk = model.Id
            }));
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

    public IEnumerable<PictureReferenceModel> GetPictureReferences()
    {
        return _dataContext.PictureCardReferences.Select(r => new PictureReferenceModel()
        {
            Id = r.Id,
            CardFk = r.CardFk,
            PictureFk = r.PictureFk
        });
    }
}
