using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace TheCardEditor.SheetComponent;

internal static class ObjectExtensions
{
    public static T? FromJson<T>(this Stream utf8Stream, bool includeFields = true, bool ignoreCycles = true, bool allowNumberReadingFromString = false) where T : class
    {
        return JsonSerializer.Deserialize<T>(utf8Stream, new JsonSerializerOptions()
        {
            IncludeFields = includeFields,
            PropertyNameCaseInsensitive = true,
            NumberHandling = allowNumberReadingFromString ? JsonNumberHandling.AllowReadingFromString : JsonNumberHandling.Strict,
            ReferenceHandler = ignoreCycles ? ReferenceHandler.IgnoreCycles : ReferenceHandler.Preserve
        });
    }

    public static T? FromJson<T>(this byte[] utf8Json, bool includeFields = true, bool ignoreCycles = true, bool allowNumberReadingFromString = false) where T : class
    {
        return JsonSerializer.Deserialize<T>(utf8Json, new JsonSerializerOptions()
        {
            IncludeFields = includeFields,
            PropertyNameCaseInsensitive = true,
            NumberHandling = allowNumberReadingFromString ? JsonNumberHandling.AllowReadingFromString : JsonNumberHandling.Strict,
            ReferenceHandler = ignoreCycles ? ReferenceHandler.IgnoreCycles : ReferenceHandler.Preserve
        });
    }

    public static T? FromJson<T>(this string json, bool includeFields = true, bool ignoreCycles = true, bool allowNumberReadingFromString = false) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions()
            {
                IncludeFields = includeFields,
                PropertyNameCaseInsensitive = true,
                NumberHandling = allowNumberReadingFromString ? JsonNumberHandling.AllowReadingFromString : JsonNumberHandling.Strict,
                ReferenceHandler = ignoreCycles ? ReferenceHandler.IgnoreCycles : ReferenceHandler.Preserve
            });
        }
        catch (Exception)
        {
            return null;
        }
    }

    public static string AsJson<T>(this T obj, bool includeFields = true, bool ignoreCycles = true)
    {
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions()
        {
            IncludeFields = includeFields,
            ReferenceHandler = ignoreCycles ? ReferenceHandler.IgnoreCycles : ReferenceHandler.Preserve
        });
    }

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
