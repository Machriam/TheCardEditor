namespace TheCardEditor.DataModel.DataModel;

public partial class Font
{
    public Font()
    {
    }

    public long Id { get; set; }
    public string Base64Data { get; set; } = null!;
    public string Name { get; set; } = null!;
}
