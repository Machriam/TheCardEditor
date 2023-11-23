namespace TheCardEditor.DataModel.DataModel;

public partial class ApplicationDatum
{
    public ApplicationDatum()
    {
    }

    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Value { get; set; } = null!;
}
