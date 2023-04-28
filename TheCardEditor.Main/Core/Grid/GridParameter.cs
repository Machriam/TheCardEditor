namespace TheCardEditor.Main.Core.Grid;

public class GridParameter
{
    public bool MultipleRowSelect { get; set; }
    public bool UseUserIds { get; set; }
    public bool SuppressFilterButton { get; set; }
    public string? RowSelectionHandler { get; set; }
    public Dictionary<string, IEnumerable<string>>? EnumParameter { get; set; }
    public bool ShowFilter { get; set; }
    public int RowSize { get; set; } = 15;
    public IEnumerable<ColumnDefinition>? ColumnDefinitions { get; set; }
}
