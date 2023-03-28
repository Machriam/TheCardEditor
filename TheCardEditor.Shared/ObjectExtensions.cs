using System.ComponentModel.DataAnnotations;

namespace TheCardEditor.Shared;

public static class ObjectExtensions
{
    public static bool ModelIsValid(this object model, out string errors)
    {
        var validationContext = new ValidationContext(model);
        var validationResults = new List<ValidationResult>();
        if (model == null) { errors = "Object was null"; return false; }
        var result = Validator.TryValidateObject(model, validationContext, validationResults, true);
        errors = string.Join(", ", validationResults.Select(vr => vr.ErrorMessage));
        return result;
    }

    public static bool ModelListIsValid<TEnumerable>(this TEnumerable model, out string errors) where TEnumerable : IEnumerable<object>
    {
        var validationResults = new List<ValidationResult>();
        errors = "";
        if (model == null || !model.Any()) return true;
        var index = 0;
        foreach (var item in model)
        {
            var validationContext = new ValidationContext(item);
            if (!Validator.TryValidateObject(item, validationContext, validationResults, true))
            {
                errors += $"First error in line: {index}\n";
                errors += string.Join(", ", validationResults.Select(vr => vr.ErrorMessage));
                return false;
            }
            index++;
        }
        return true;
    }
}
