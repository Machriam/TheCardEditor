﻿namespace TheCardEditor.Main.Core;

public class AppSettings
{
    public static string GetPath => Path.Combine(Path.GetFullPath(Directory.GetCurrentDirectory()), Name);
#if DEBUG
    public const string Name = "appsettings.Development.json";
#else
    public const string Name = "appsettings.json";
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
