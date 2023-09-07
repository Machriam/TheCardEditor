using TheCardEditor.DataModel.DataModel;
using TheCardEditor.DataModel.DTO;

namespace TheCardEditor.Services;

public class TemplateService
{
    private readonly DataContext _dataContext;

    public TemplateService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public IEnumerable<string> GetTemplateNames(long cardSetId)
    {
        return _dataContext.Templates.Where(t => t.CardSetFk == cardSetId).Select(t => t.Name ?? "");
    }

    public TemplateModel GetTemplate(long id)
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

    public void DeleteTemplate(long id)
    {
        _dataContext.Templates.Remove(_dataContext.Templates.First(t => t.Id == id));
        _dataContext.SaveChanges();
    }
}
