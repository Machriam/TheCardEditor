using System.Reflection;
using System.Text.Json.Serialization;

namespace TheCardEditor.SheetComponent;

public abstract class AbstractSheetModel
{
    private record struct AttributeProperty(SheetMetaData MetaData, string Name);

    [JsonIgnore()]
    public IEnumerable<SheetColumnDefinition> ColumnDefinitions
    {
        get
        {
            var properties = GetType().GetProperties()
                .Where(p => Attribute.IsDefined(p, typeof(SheetMetaData)))
                .Select(p => new AttributeProperty((SheetMetaData)p.GetCustomAttribute(typeof(SheetMetaData))!, p.Name));
            return properties
                .Select(p => new SheetColumnDefinition(p.MetaData.HeaderName, p.MetaData.Width, p.MetaData.Editable,
                p.MetaData.SheetConverter, p.Name));
        }
    }
}
