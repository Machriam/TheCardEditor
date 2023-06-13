namespace TheCardEditor.Main.Core.Grid;

public class DisplayGridModel<TData> where TData : AbstractGridModel
{
    public DisplayGridModel(IEnumerable<TData> data, bool suppressFilterButton = false, int rowsize = 20,
        bool showFilter = true, Dictionary<string, IEnumerable<string>>? enumParameter = null, bool useUserIds = false,
        Dictionary<string, List<string>>? dynamicColumns = null)
    {
        Parameter = new GridParameter()
        {
            ColumnDefinitions = ((TData?)Activator.CreateInstance(typeof(TData), 0))!.ColumnDefinitions,
            SuppressFilterButton = suppressFilterButton,
            RowSize = rowsize,
            ShowFilter = showFilter,
            UseUserIds = useUserIds,
            EnumParameter = enumParameter,
        };
        foreach (var column in dynamicColumns ?? new())
        {
            var template = Parameter.ColumnDefinitions
                .First(cd => cd.Field.Equals(column.Key, StringComparison.InvariantCultureIgnoreCase));
            foreach (var field in column.Value)
            {
                var templateCopy = template.Clone();
                templateCopy.Field = field;
                templateCopy.HeaderName = field;
                templateCopy.Hide = false;
                Parameter.ColumnDefinitions = Parameter.ColumnDefinitions.Append(templateCopy);
            }
        }
        Data = data;
    }

    public GridParameter Parameter { get; }
    public IEnumerable<TData> Data { get; }
}
