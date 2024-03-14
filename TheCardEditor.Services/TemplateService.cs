using TheCardEditor.DataModel.DataModel;
using TheCardEditor.Shared.DTO;

namespace TheCardEditor.Services;

public class TemplateService
{
    private readonly DataContext _dataContext;

    public TemplateService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public IReadOnlyDictionary<int, string> TemplateNamesById(int cardSetId)
    {
        return _dataContext.Templates.Where(t => t.CardSetFk == cardSetId).ToDictionary(t => t.Id, t => t.Name);
    }

    public TemplateModel GetTemplate(int id)
    {
        return new TemplateModel(_dataContext.Templates.First(t => t.Id == id));
    }

    public void StoreTemplate(string name, CardModel card)
    {
        _dataContext.Templates.Add(new Template()
        {
            Name = name,
            Data = card.VirtualData,
            CardSetFk = card.CardSetFk,
        });
        _dataContext.SaveChanges();
    }

    public void DeleteTemplate(int id)
    {
        _dataContext.Templates.Remove(_dataContext.Templates.First(t => t.Id == id));
        _dataContext.SaveChanges();
    }
}
