using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheCardEditor.Shared.Features.CardEditor;

public enum FilterParameterType
{
    Int = 0,
    Bool = 1,
    String = 2,
    Double = 3,
}

public class FilterParameter
{
    public object ParsedValue => Type switch
    {
        FilterParameterType.Int => int.Parse(Value),
        FilterParameterType.Bool => bool.Parse(Value),
        FilterParameterType.String => Value,
        FilterParameterType.Double => double.Parse(Value.Replace(",", "."), CultureInfo.InvariantCulture),
        _ => new(),
    };

    public string Name { get; set; } = "";
    public FilterParameterType Type { get; set; }
    public string Value { get; set; } = "";
}

public class ImageFilterModel
{
    public string Name { get; set; } = "";
    public IEnumerable<FilterParameter> Parameters { get; set; } = [];
}

public class ImageFilterPipeline
{
    public IEnumerable<ImageFilterModel> Filters { get; set; } = [];
}
