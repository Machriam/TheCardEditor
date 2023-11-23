using System.Collections;
using System.Globalization;
using System.Resources;

namespace TheCardEditor.DataModel.Migrations;

public static class IVersionSortExtensions
{
    public static IOrderedEnumerable<(int[] Version, string SQL)> GetPatchesToApply(this IVersionSort version, ResourceSet set)
    {
        var currentVersionNumber = version.CurrentVersion.Split(".").Select(int.Parse).ToArray();
        var migrations = new List<string>();
        var versions = new List<(int[] Version, string SQL)>();
        foreach (DictionaryEntry entry in set ?? throw new Exception("No Migrations found"))
        {
            var resourceVersion = entry.Key.ToString()?.Split(".").Select(int.Parse) ?? throw new Exception("Invalid Migration entry");
            versions.Add((resourceVersion.ToArray(), entry.Value?.ToString() ?? throw new Exception("Invalid Migration entry")));
        }
        return versions.Where(v => v.Version[0] > currentVersionNumber[0] ||
                           (v.Version[0] == currentVersionNumber[0] && v.Version[1] > currentVersionNumber[1]) ||
                           (v.Version[0] == currentVersionNumber[0] && v.Version[1] == currentVersionNumber[1] && v.Version[2] > currentVersionNumber[2]))
                .OrderBy(v => v.Version[0])
                .ThenBy(v => v.Version[1])
                .ThenBy(v => v.Version[2]);
    }
}

public interface IVersionSort
{
    private class VersionSortModel : IVersionSort
    {
        public string CurrentVersion { get; set; } = "0.0.0";
    }

    public static IVersionSort CreateDefault(string currentVersion)
    {
        return new VersionSortModel { CurrentVersion = currentVersion };
    }

    public string CurrentVersion { get; }
}
