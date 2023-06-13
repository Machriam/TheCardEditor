using System.Text.Json.Serialization;
using TheCardEditor.Shared;

namespace TheCardEditor.Main.Core.Grid;

public class AgGridFilterParams
{
    [JsonPropertyName("defaultOption")]
    public string? DefaultOption { get; set; }
}

public class ColumnDefinition
{
    public ColumnDefinition Clone()
    {
        return (ColumnDefinition)MemberwiseClone();
    }
    public ColumnDefinition(string? headerName, string field, int width, FilterParams filterParams,
        bool hide, bool editable, bool filter, bool resizable, bool autoHeight, bool sortable,
        bool wrapText, CellRenderer cellRenderer, string tooltip)
    {
        HeaderName = headerName ?? "";
        Field = field;
        if (width <= 0) Width = null;
        else Width = width;
        FilterParams = filterParams;
        HeaderTooltip = tooltip;
        Hide = hide;
        Editable = editable;
        Filter = filter ? GetFilter(filterParams) : false;
        Resizable = resizable;
        AutoHeight = autoHeight;
        Sortable = sortable;
        WrapText = wrapText;
        CellRenderer = cellRenderer.GetDescription();
    }

    private static object GetFilter(FilterParams filter) => filter switch
    {
        FilterParams.StartsWith => true,
        FilterParams.Contains => true,
        FilterParams.GreaterThan => "agNumberColumnFilter",
        _ => throw new NotImplementedException()
    };

    [JsonPropertyName("headerName")]
    public string HeaderName { get; set; }

    [JsonPropertyName("field")]
    public string Field { get; set; }

    [JsonPropertyName("width")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? Width { get; set; }

    [JsonPropertyName("filter")]
    public object Filter { get; set; }

    [JsonPropertyName("resizable")]
    public bool Resizable { get; set; }

    [JsonPropertyName("headerTooltip")]
    public string HeaderTooltip { get; set; }

    [JsonPropertyName("autoHeight")]
    public bool AutoHeight { get; set; }

    [JsonPropertyName("sortable")]
    public bool Sortable { get; set; } = true;

    [JsonPropertyName("wrapText")]
    public bool WrapText { get; set; }

    [JsonPropertyName("filterParams")]
    public AgGridFilterParams AgGridFilter => new()
    {
        DefaultOption = FilterParams.GetDescription()
    };

    [JsonIgnore()]
    public FilterParams FilterParams { get; set; }

    [JsonPropertyName("hide")]
    public bool Hide { get; set; }

    [JsonPropertyName("editable")]
    public bool Editable { get; set; }

    [JsonPropertyName("cellRenderer")]
    public string CellRenderer { get; set; }
}
