using System.ComponentModel;
using System.Text.Json.Serialization;

namespace TheCardEditor.SheetComponent;

public enum SheetConverter
{
    [Description("default")]
    Default,

    [Description("numeric")]
    Numeric,
}

public class SheetColumnDefinition(string? headerName, int width, bool editable, SheetConverter sheetConverter, string propertyName)
{
    public string PropertyName { get; set; } = propertyName;
    public string HeaderName { get; set; } = headerName ?? "";

    public int Width { get; set; } = width;

    public bool Editable { get; set; } = editable;
    public string? Converter => SheetConverter.GetDescription();

    [JsonIgnore()]
    public SheetConverter SheetConverter { get; set; } = sheetConverter;
}
