using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TheCardEditor.Main.Core;

public class JsModuleInvalidator : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private FileSystemWatcher? _watcher;

    public JsModuleInvalidator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _watcher = new FileSystemWatcher
        {
            Path = Directory.GetCurrentDirectory() + "/wwwroot/lib",
            Filter = "*.*",
            NotifyFilter = NotifyFilters.LastWrite
        };
        _watcher.Changed += FileWatcher_Changed;
        _watcher.EnableRaisingEvents = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _watcher?.Dispose();
        return Task.CompletedTask;
    }

    public void FileWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        _serviceProvider.GetRequiredService<IJsInterop>().InvalidateModules();
    }
}
