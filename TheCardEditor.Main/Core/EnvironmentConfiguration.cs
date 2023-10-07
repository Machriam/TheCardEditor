using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TheCardEditor.Main.Core;

public interface IEnvironmentConfiguration
{
    string DatabasePath { get; }
    WindowPosition WindowPosition { get; }

    void SaveNewWindowPosition(WindowPosition newWindowPosition);
}

public class EnvironmentConfiguration : IEnvironmentConfiguration
{
    private readonly IConfiguration _configuration;

    public EnvironmentConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string DatabasePath => _configuration.GetConnectionString(nameof(DatabasePath)) ?? throw new Exception("No database path defined");

    public WindowPosition WindowPosition => _configuration.GetSection(WindowPosition.Key).Get<WindowPosition>() ?? new();

    public void SaveNewWindowPosition(WindowPosition newWindowPosition)
    {
        var appsettings = File.ReadAllText(AppSettings.GetPath);
        var data = JsonSerializer.Deserialize<AppSettings>(appsettings) ?? new();
        data.WindowPosition = newWindowPosition;
        File.WriteAllText(AppSettings.GetPath, JsonSerializer.Serialize(data, new JsonSerializerOptions() { WriteIndented = true }));
    }
}
