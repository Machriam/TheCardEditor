using System.ComponentModel;

namespace TheCardEditor.Main.Core.Grid;

public static class BarDataExtensions
{
    public static string GetBarDataString(this IEnumerable<BarData> data)
    {
        return string.Join(";", data.Select(d => d.Value + ":" + d.Color + ":" + d.Annotation));
    }
}

public struct BarData
{
    public BarData(decimal value, string color, string? annotation = null)
    {
        Color = color;
        Value = value;
        Annotation = annotation;
    }

    public BarData(double value, string color, string? annotation = null)
    {
        Color = color;
        Value = (decimal)value;
        Annotation = annotation;
    }

    public string Color { get; set; }
    public decimal Value { get; set; }
    public string? Annotation { get; set; }
}

public enum CellRenderer
{
    [Description("")]
    Default,

    [Description("newLine")]
    NewLineBreaks,

    [Description("xAmount")]
    XAmount,

    [Description("percent")]
    Percent,

    [Description("enum")]
    Enum,

    [Description("bar")]
    Bar,

    [Description("barPercentage")]
    BarPercentage
}

public enum FilterParams
{
    [Description("startsWith")]
    StartsWith,

    [Description("contains")]
    Contains,

    [Description("greaterThan")]
    GreaterThan
}

public class GridMetaData : Attribute
{
    public bool Editable { get; set; }
    public bool Hide { get; set; }
    public int Width { get; set; } = -1;
    public string? HeaderName { get; set; }
    public FilterParams FilterParams { get; set; } = FilterParams.Contains;
    public bool Filter { get; set; } = true;
    public bool Resizable { get; set; }
    public string Tooltip { get; set; } = "";
    public bool AutoHeight { get; set; }
    public bool Sortable { get; set; } = true;
    public bool WrapText { get; set; }
    public CellRenderer CellRenderer { get; set; } = CellRenderer.Default;
}
