using TheCardEditor.DataModel.DataModel;
using TheCardEditor.Shared.DTO;

namespace TheCardEditor.Services;

public class FontService
{
    private readonly DataContext _dataContext;

    public FontService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public void DeleteFont(FontModel font)
    {
        _dataContext.Fonts.Remove(_dataContext.Fonts.First(f => f.Id == font.Id));
        _dataContext.SaveChanges();
    }

    public void UpdateFont(FontModel model)
    {
        var font = _dataContext.Fonts.FirstOrDefault(f => f.Id == model.Id);
        if (font == null) _dataContext.Fonts.Add(model.GetDataModel());
        else
        {
            font.Name = model.Name;
            font.Base64Data = model.Base64Data;
        }
        _dataContext.SaveChanges();
    }

    public IEnumerable<FontModel> GetFonts()
    {
        return _dataContext.Fonts.Select(f => new FontModel(f));
    }
}
