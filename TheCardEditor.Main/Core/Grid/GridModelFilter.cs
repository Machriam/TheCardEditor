using System.Text.Json.Serialization;

namespace TheCardEditor.Main.Core.Grid;

public class GridModelFilter
{
    public GridModelFilter()
    { }

    public GridModelFilter(string columnName, object value, FilterType filterType)
    {
        Type = filterType;
        Name = char.ToLower(columnName[0]) + (columnName.Length > 1 ? columnName[1..] : "");
        Value = value;
    }

    public enum FilterType
    {
        NumericGreaterThan
    }

    [JsonIgnore()]
    public FilterType Type { get; set; }

    public string? Name { get; set; }

    [JsonIgnore()]
    public object? Value { get; set; }

    public object? Filter => Type switch
    {
        FilterType.NumericGreaterThan => new { FilterType = "agNumberColumnFilter", Type = "greaterThan", Filter = Value },
        _ => throw new NotImplementedException()
    };
}
