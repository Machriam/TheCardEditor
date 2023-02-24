using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TheCardEditor.DataModel;

namespace TheCardEditor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
#if DEBUG
        const string AppsettingsName = "appsettings.Development.json";
        var directory = Path.GetFullPath(Directory.GetCurrentDirectory());
#else
        const string AppsettingsName = "appsettings.json";
        var directory = Directory.GetCurrentDirectory();
#endif
        var builder = new ConfigurationBuilder()
                        .SetBasePath(directory)
                        .AddJsonFile(AppsettingsName, optional: false, reloadOnChange: true);
        var configuration = builder.Build();
        var environmentConfiguration = new EnvironmentConfiguration(configuration);
        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();
        services.AddEntityFrameworkSqlite();
        services.AddDbContext<DataContext>(opt =>
                    opt.UseSqlite($"Data Source={environmentConfiguration.DatabasePath}", a =>
        {
            a.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        }));
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
        services.AddSingleton<MainForm>();
        services.AddTransient<IErrorLogger, JsInterop>();
        var mainForm = services.BuildServiceProvider().GetService<MainForm>();
        Application.Run(mainForm);
    }
}
