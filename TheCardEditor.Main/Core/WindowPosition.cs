using System.Collections.Frozen;

namespace TheCardEditor.Main.Core;

public class AppSettings
{
    public static readonly FrozenSet<string> AllowedPictureTypes = new[] { ".png", ".jpg", ".jpeg" }.SelectMany(x => new[] { x.ToUpper(), x }).ToFrozenSet();
    public static string GetPath => Path.Combine(Path.GetFullPath(Directory.GetCurrentDirectory()), AppsettingsName);
#if DEBUG
    public const string AppsettingsName = "appsettings.Development.json";
#else
    public const string AppsettingsName = "appsettings.json";
#endif
    public const string Key = nameof(AppSettings);
    public Dictionary<string, string> ConnectionStrings { get; set; } = new();
    public WindowPosition WindowPosition { get; set; } = new();
}

public class WindowPosition
{
    public const string Key = nameof(WindowPosition);
    public int LocationX { get; set; } = 100;
    public int LocationY { get; set; } = 100;
    public FormWindowState State { get; set; } = FormWindowState.Normal;
    public int SizeX { get; set; } = 1000;
    public int SizeY { get; set; } = 600;
}
