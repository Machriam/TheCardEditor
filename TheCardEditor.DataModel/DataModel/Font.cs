namespace TheCardEditor.DataModel.DataModel;

public partial class Font
{
    public Font()
    {
    }

    public int Id { get; set; }
    public string Base64Data { get; set; } = null!;
    public string Name { get; set; } = null!;
}
