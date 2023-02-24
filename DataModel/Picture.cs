namespace TheCardEditor.DataModel;

public partial class Picture
{
    public Picture()
    {
    }

    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public virtual ICollection<Layer> Layers { get; init; } = new List<Layer>();
}
