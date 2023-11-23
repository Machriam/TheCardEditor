namespace TheCardEditor.DataModel.DataModel;

public partial class Template
{
    public Template()
    {
    }

    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Data { get; set; } = null!;
    public int CardSetFk { get; set; }
    public virtual CardSet CardSetFkNavigation { get; set; } = null!;
}
