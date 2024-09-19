using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TheCardEditor.Shared;

namespace TheCardEditor.Main.Core;

public interface IEnvironmentConfiguration
{
    string DatabasePath { get; }
    WindowPosition WindowPosition { get; }

    void SaveNewWindowPosition(WindowPosition newWindowPosition);

    void SaveDatabaseLocation(string location);
}

public class EnvironmentConfiguration : IEnvironmentConfiguration
{
    private readonly IConfiguration _configuration;

    public EnvironmentConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string DatabasePath
    {
        get
        {
            var connection = _configuration.GetConnectionString(nameof(DatabasePath)) ?? "";
            if (!connection.Pipe(File.Exists))
                return Environment.SpecialFolder.MyDocuments
                    .Pipe(Environment.GetFolderPath)
                    .Pipe(x => Path.Combine(x, "CardEditor.sqlite3"));
            return connection;
        }
    }

    public WindowPosition WindowPosition => _configuration.GetSection(WindowPosition.Key).Get<WindowPosition>() ?? new();

    public void SaveDatabaseLocation(string location)
    {
        var appsettings = File.ReadAllText(AppSettings.GetPath);
        var data = JsonSerializer.Deserialize<AppSettings>(appsettings) ?? new();
        data.ConnectionStrings[nameof(DatabasePath)] = location;
        File.WriteAllText(AppSettings.GetPath, JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true }));
    }

    public void SaveNewWindowPosition(WindowPosition newWindowPosition)
    {
        var appsettings = File.ReadAllText(AppSettings.GetPath);
        var data = JsonSerializer.Deserialize<AppSettings>(appsettings) ?? new();
        data.WindowPosition = newWindowPosition;
        File.WriteAllText(AppSettings.GetPath, JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true }));
    }
}
