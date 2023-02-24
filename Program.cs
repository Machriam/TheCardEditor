using Microsoft.Extensions.DependencyInjection;

namespace TheCardEditor;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        var services = new ServiceCollection();
        services.AddWindowsFormsBlazorWebView();
#if DEBUG
        services.AddBlazorWebViewDeveloperTools();
#endif
        services.AddSingleton<MainForm>();
        var mainForm = services.BuildServiceProvider().GetService<MainForm>();
        Application.Run(mainForm);
    }
}
