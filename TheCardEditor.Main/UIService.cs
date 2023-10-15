using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace TheCardEditor.Main;

public class UIService : IHostedService
{
    private readonly IServiceProvider _services;

    public UIService(IServiceProvider services)
    {
        _services = services;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var mainForm = _services.GetRequiredService<MainForm>();
        Application.Run(mainForm);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
