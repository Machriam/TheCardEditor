﻿using System.Text;
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

    private IEnumerable<string> RecursivePictureLoad(string path)
    {
        var result = new List<string>();
        foreach (var file in Directory.GetFiles(path))
        {
            if (file.EndsWith(".png"))
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

    public void LoadPicturesFromPath(string path)
    {
        var result = RecursivePictureLoad(path);
        var importedPictures = _dataContext.Pictures.Select(p => p.Path).ToHashSet();
        foreach (var newPicture in result.Where(r => !importedPictures.Contains(r)))
        {
            _dataContext.Pictures.Add(new Picture()
            {
                Name = Path.GetFileNameWithoutExtension(newPicture),
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

    public string GetBase64Picture(long pictureId)
    {
        var picture = _dataContext.Pictures.FirstOrDefault(p => p.Id == pictureId);
        if (!Path.Exists(picture?.Path)) return "";
        using var memoryStream = new MemoryStream();
        File.OpenRead(picture.Path).CopyTo(memoryStream);
        return new StringBuilder("data:image/png;base64,").Append(Convert.ToBase64String(memoryStream.ToArray())).ToString();
    }
}
