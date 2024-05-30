namespace TheCardEditor.SheetComponent;

public class SheetMetaData : Attribute
{
    public bool Editable { get; set; } = true;
    public int Width { get; set; } = 200;
    public string? HeaderName { get; set; }
    public SheetConverter SheetConverter { get; set; } = SheetConverter.Default;
}
