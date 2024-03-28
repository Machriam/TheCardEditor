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

    public ImageFilterSelector For() => new(this);
}

public class ImageFilterSelector(ImageFilterModel model)
{
    public ImageFilterModel FormFree()
    {
        model.Name = "TransparentFilter";
        model.Parameters = [];
        return model;
    }

    public ImageFilterModel InvertColors()
    {
        model.Name = "InvertColors";
        model.Parameters = [];
        return model;
    }

    public ImageFilterModel Canny(double threshold1 = 100d, double threshold2 = 300d, int aperture = 3, bool l2Gradient = false)
    {
        model.Name = "Canny";
        model.Parameters = [
            new FilterParameter() { Name="Threshold 1",Type=FilterParameterType.Double,Value=$"{threshold1}" },
            new FilterParameter() { Name="Threshold 2",Type=FilterParameterType.Double,Value=$"{threshold2}" },
            new FilterParameter() { Name="Aperture Size",Type=FilterParameterType.Int,Value=$"{aperture}" },
            new FilterParameter() { Name="L2 Gradient",Type=FilterParameterType.Bool,Value=l2Gradient.ToString() },
        ];
        return model;
    }
}

public class ImageFilterPipeline
{
    public IEnumerable<ImageFilterModel> Filters { get; set; } = [];
}
