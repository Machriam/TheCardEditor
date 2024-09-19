namespace TheCardEditor.SheetComponent;

public record struct HighlightData(string Color);

public class DisplaySheetModel<TData> where TData : AbstractSheetModel
{
    public DisplaySheetModel(IEnumerable<TData> data, Dictionary<string, string[]>? validValuesFor = null,
        HighlightData[]? highlightCellsDictionary = null, int minimumRows = 1,
        Dictionary<string, List<string>>? dynamicColumns = null)
    {
        Parameter = new SheetParameter()
        {
            ColumnDefinitions = ((TData?)Activator.CreateInstance(typeof(TData)))!.ColumnDefinitions,
            AllowedValuesFor = validValuesFor,
            PossibleRowColors = highlightCellsDictionary ?? [],
            MinimumRows = minimumRows
        };
        foreach (var column in dynamicColumns ?? new())
        {
            var template = Parameter.ColumnDefinitions
                .First(cd => cd.HeaderName.Equals(column.Key, StringComparison.InvariantCultureIgnoreCase));
            foreach (var field in column.Value)
            {
                var templateCopy = template.Clone();
                templateCopy.HeaderName = field;
                templateCopy.IsDynamicColumn = true;
                Parameter.ColumnDefinitions = Parameter.ColumnDefinitions.Append(templateCopy);
            }
            Parameter.ColumnDefinitions = Parameter.ColumnDefinitions.Where(cd => cd.HeaderName != column.Key);
        }
        Data = data;
    }

    public SheetParameter Parameter { get; }
    public IEnumerable<TData> Data { get; }
}
