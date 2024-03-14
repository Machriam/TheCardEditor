using TheCardEditor.Shared;

namespace TheCardEditor.Shared.DTO;

public class PictureModel : IDataListItem
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string UniqueName => DuplicatedName ? $"{Name}({System.IO.Path.GetDirectoryName(Path)})" : Name;
    public bool DuplicatedName { get; set; }

    public string? GetText => UniqueName;
}
