namespace TheCardEditor.SheetComponent;

public class DisplaySheetModel<TData> where TData : AbstractSheetModel
{
    public DisplaySheetModel(IEnumerable<TData> data, Dictionary<string, string[]>? validValuesFor = null, int minimumRows = 1)
    {
        Parameter = new SheetParameter()
        {
            ColumnDefinitions = ((TData?)Activator.CreateInstance(typeof(TData)))!.ColumnDefinitions,
            AllowedValuesFor = validValuesFor,
            MinimumRows = minimumRows
        };
        Data = data;
    }

    public SheetParameter Parameter { get; }
    public IEnumerable<TData> Data { get; }
}
