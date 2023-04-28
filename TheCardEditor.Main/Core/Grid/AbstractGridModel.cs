using System.Reflection;
using System.Text.Json.Serialization;

namespace TheCardEditor.Main.Core.Grid;

public abstract class AbstractGridModel
{
    private struct AttributeProperty
    {
        public GridMetaData MetaData { get; }
        public string Name { get; }

        public AttributeProperty(GridMetaData metaData, string name)
        {
            MetaData = metaData;
            Name = name;
        }
    }

    public string? RowColorClass { get; set; }
    public string? RowTooltip { get; set; }

    [JsonIgnore()]
    public IEnumerable<ColumnDefinition> ColumnDefinitions
    {
        get
        {
            var properties = GetType().GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(GridMetaData)))
                .Select(p => new AttributeProperty((GridMetaData)p.GetCustomAttribute(typeof(GridMetaData))!, p.Name));
            return properties
                .Select(p => new ColumnDefinition(p.MetaData.HeaderName, p.Name.Length > 0 ? char.ToLower(p.Name[0]) + p.Name[1..] : p.Name,
                             p.MetaData.Width, p.MetaData.FilterParams, p.MetaData.Hide, p.MetaData.Editable,
                             p.MetaData.Filter, p.MetaData.Resizable, p.MetaData.AutoHeight, p.MetaData.Sortable, p.MetaData.WrapText,
                             p.MetaData.CellRenderer, p.MetaData.Tooltip));
        }
    }

    protected AbstractGridModel(long id)
    {
        Id = id;
    }

    [GridMetaData(Hide = true, HeaderName = "Id")]
    public long Id { get; set; }
}
