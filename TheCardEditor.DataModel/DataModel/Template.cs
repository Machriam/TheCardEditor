namespace TheCardEditor.DataModel.DataModel;

public partial class Template
{
    public Template()
    {
    }

    public long Id { get; set; }
    public string? Name { get; set; }
    public string Data { get; set; } = null!;
}
