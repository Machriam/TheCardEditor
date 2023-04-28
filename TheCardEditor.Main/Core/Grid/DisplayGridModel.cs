namespace TheCardEditor.Main.Core.Grid;

public class DisplayGridModel<TData> where TData : AbstractGridModel
{
    public DisplayGridModel(IEnumerable<TData> data, bool suppressFilterButton = false, int rowsize = 20,
        bool showFilter = true, Dictionary<string, IEnumerable<string>>? enumParameter = null, bool useUserIds = false)
    {
        Parameter = new GridParameter()
        {
            ColumnDefinitions = ((TData?)Activator.CreateInstance(typeof(TData), 0))!.ColumnDefinitions,
            SuppressFilterButton = suppressFilterButton,
            RowSize = rowsize,
            ShowFilter = showFilter,
            UseUserIds = useUserIds,
            EnumParameter = enumParameter
        };
        Data = data;
    }

    public GridParameter Parameter { get; }
    public IEnumerable<TData> Data { get; }
}
