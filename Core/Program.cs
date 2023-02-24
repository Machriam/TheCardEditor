using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace TheCardEditor;
public class EnvironmentConfiguration
{
    private readonly IConfiguration _configuration;

    public EnvironmentConfiguration(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string DatabasePath => _configuration.GetConnectionString(nameof(DatabasePath)) ?? throw new Exception("No database path defined");
}
