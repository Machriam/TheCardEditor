namespace TheCardEditor.DataModel.DataModel;

public partial class Template
{
    public Template()
    {
    }

    public long Id { get; set; }
    public string Name { get; set; } = null!;
    public string Data { get; set; } = null!;
    public long CardSetFk { get; set; }
    public virtual CardSet CardSetFkNavigation { get; set; } = null!;
}
