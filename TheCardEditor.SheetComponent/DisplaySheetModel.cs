namespace TheCardEditor.SheetComponent;

public record struct HighlightData(string Value, string Color, bool WholeRow);

public class DisplaySheetModel<TData> where TData : AbstractSheetModel
{
    public DisplaySheetModel(IEnumerable<TData> data, Dictionary<string, string[]>? validValuesFor = null,
        Dictionary<string, HighlightData[]>? highlightCellsDictionary = null, int minimumRows = 1)
    {
        Parameter = new SheetParameter()
        {
            ColumnDefinitions = ((TData?)Activator.CreateInstance(typeof(TData)))!.ColumnDefinitions,
            AllowedValuesFor = validValuesFor,
            HighlightCellsDictionary = highlightCellsDictionary?
                .ToDictionary(d => d.Key, d => d.Value.GroupBy(v => v.Value)
                .ToDictionary(v => v.Key, v => v.FirstOrDefault())),
            MinimumRows = minimumRows
        };
        Data = data;
    }

    public SheetParameter Parameter { get; }
    public IEnumerable<TData> Data { get; }
}
