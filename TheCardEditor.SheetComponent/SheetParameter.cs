namespace TheCardEditor.SheetComponent;

public class SheetParameter
{
    public IEnumerable<SheetColumnDefinition>? ColumnDefinitions { get; set; }
    public Dictionary<string, string[]>? AllowedValuesFor { get; set; }
    public int MinimumRows { get; set; }
}
