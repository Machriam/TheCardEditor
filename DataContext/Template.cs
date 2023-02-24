namespace TheCardEditor.DataContext;

public partial class Template
{
    public Template()
    {
    }

    public long Id { get; set; }
    public string? Name { get; set; }
    public virtual ICollection<TemplateLayerAssociation> TemplateLayerAssociations { get; init; } = new List<TemplateLayerAssociation>();
}
