namespace TheCardEditor.DataContext;

public partial class Layer
{
    public Layer()
    {
    }

    public long Id { get; set; }
    public long WidthOffset { get; set; }
    public long HeightOffset { get; set; }
    public long PictureFk { get; set; }
    public string HtmlTemplate { get; set; } = null!;
    public long CardFk { get; set; }
    public virtual Card CardFkNavigation { get; set; } = null!;
    public virtual Picture PictureFkNavigation { get; set; } = null!;
    public virtual ICollection<TemplateLayerAssociation> TemplateLayerAssociations { get; init; } = new List<TemplateLayerAssociation>();
}
