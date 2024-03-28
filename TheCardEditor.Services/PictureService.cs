using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using TheCardEditor.DataModel.DataModel;
using TheCardEditor.Shared.DTO;

namespace TheCardEditor.Services;

public class PictureService(DataContext dataContext)
{
    private readonly DataContext _dataContext = dataContext;
    private static readonly string[] s_allowedFiles = [".png", ".jpg", ".JPEG"];

    private IEnumerable<string> RecursivePictureLoad(string path)
    {
        var result = new List<string>();
        foreach (var file in Directory.GetFiles(path))
        {
            if (s_allowedFiles.Any(af => file.EndsWith(af, StringComparison.InvariantCultureIgnoreCase)))
            {
                result.Add(file);
            }
        }
        foreach (var dir in Directory.GetDirectories(path))
        {
            result.AddRange(RecursivePictureLoad(dir));
        }
        return result;
    }

    public void DeletePicture(long id)
    {
        _dataContext.Pictures.Remove(_dataContext.Pictures.First(p => p.Id == id));
        _dataContext.SaveChanges();
    }

    public void LoadPicturesFromPath(string path)
    {
        var result = RecursivePictureLoad(path);
        var importedPictures = _dataContext.Pictures.Select(p => p.Path).ToHashSet();
        foreach (var newPicture in result.Where(r => !importedPictures.Contains(r)))
        {
            _dataContext.Pictures.Add(new Picture()
            {
                Name = Path.GetFileName(newPicture),
                Path = newPicture
            });
        }
        _dataContext.SaveChanges();
    }

    public IEnumerable<PictureModel> GetPictures()
    {
        var existingNames = new Dictionary<string, PictureModel>();
        return _dataContext.Pictures.Select(p => new PictureModel()
        {
            Id = p.Id,
            Name = p.Name,
            Path = p.Path
        }).ToList()
        .Select(p =>
        {
            if (existingNames.ContainsKey(p.Name))
            {
                p.DuplicatedName = true;
                existingNames[p.Name].DuplicatedName = true;
            }
            else
            {
                existingNames.Add(p.Name, p);
            }
            return p;
        });
    }

    public Dictionary<long, bool> ValidatePictures()
    {
        var pictures = GetPictures();
        return pictures.Select(p => (p.Id, Path.Exists(p.Path)))
            .ToDictionary(p => p.Id, p => p.Item2);
    }

    public void UpdatePath(long pictureId, string newPath)
    {
        var picture = _dataContext.Pictures.First(p => p.Id == pictureId);
        picture.Path = newPath;
        picture.Name = Path.GetFileName(newPath);
        _dataContext.SaveChanges();
    }

    public string GetBase64Picture(long pictureId)
    {
        var picture = _dataContext.Pictures.FirstOrDefault(p => p.Id == pictureId);
        if (!Path.Exists(picture?.Path)) return "";
        using var memoryStream = new MemoryStream();
        File.OpenRead(picture.Path).CopyTo(memoryStream);
        return new StringBuilder("data:image/png;base64,").Append(Convert.ToBase64String(memoryStream.ToArray())).ToString();
    }
}
