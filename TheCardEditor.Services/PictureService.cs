using TheCardEditor.DataModel.DataModel;
using TheCardEditor.DataModel.DTO;

namespace TheCardEditor.Services;

public class PictureService
{
    private readonly DataContext _dataContext;

    public PictureService(DataContext dataContext)
    {
        _dataContext = dataContext;
    }

    public void LoadPicturesFromPath(string path, bool originalCall = true)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            if (file.EndsWith(".png"))
            {
                _dataContext.Pictures.Add(new Picture()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Path = file
                });
            }
        }
        foreach (var dir in Directory.GetDirectories(path))
        {
            LoadPicturesFromPath(dir, false);
        }
        if (originalCall) _dataContext.SaveChanges();
    }

    public IEnumerable<PictureModel> GetPictures()
    {
        return _dataContext.Pictures.Select(p => new PictureModel()
        {
            Id = p.Id,
            Name = p.Name,
            Path = p.Path
        });
    }

    public string GetBase64Picture(long pictureId)
    {
        var picture = _dataContext.Pictures.FirstOrDefault(p => p.Id == pictureId);
        if (!Path.Exists(picture?.Path)) return "";
        using var memoryStream = new MemoryStream();
        File.OpenRead(picture.Path).CopyTo(memoryStream);
        return Convert.ToBase64String(memoryStream.ToArray());
    }
}
