namespace TheCardEditor.Shared;

public static partial class StringExtensions
{
    public static string? GetApplicationRootGitPath(this string folderToSearchFrom)
    {
        while (!Directory.EnumerateDirectories(folderToSearchFrom, ".git").Any())
        {
            var parent = Directory.GetParent(folderToSearchFrom)?.FullName;
            if (parent == null) return null;
            folderToSearchFrom = Path.GetFullPath(parent);
        }
        return Path.GetFullPath(folderToSearchFrom);
    }

    public static bool IsEmpty(this string? value)
    {
        return string.IsNullOrEmpty(value);
    }
}
