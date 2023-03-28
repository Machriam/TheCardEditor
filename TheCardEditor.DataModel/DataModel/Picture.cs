namespace TheCardEditor.DataModel.DataModel;

public partial class Picture
{
    public Picture()
    {
    }

    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
}
