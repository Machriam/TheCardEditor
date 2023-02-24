namespace TheCardEditor.DataContext;

public partial class TemplateLayerAssociation
{
    public TemplateLayerAssociation()
    {
    }

    public long Id { get; set; }
    public long TemplateFk { get; set; }
    public long LayerFk { get; set; }
    public virtual Layer LayerFkNavigation { get; set; } = null!;
    public virtual Template TemplateFkNavigation { get; set; } = null!;
}
