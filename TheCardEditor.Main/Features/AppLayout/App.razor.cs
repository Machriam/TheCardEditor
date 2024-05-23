using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;

namespace TheCardEditor.Main.Features.AppLayout;

public partial class App
{
    [Inject] private IModalHelper ModalHelper { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    private List<Func<IModalHelper, Task<ModalResult?>>> _modalFunctions = [];
    private List<System.Threading.Timer> _timers = [];
    private bool _navigationRefresh;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += NavigationManager_LocationChanged;
        ModalHelper.DispatchGlobalModal += OnGlobalModalDispatched;
    }

    private void NavigationManager_LocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        _navigationRefresh = true;
        if (_modalFunctions.Count != 0)
        {
            foreach (var timer in _timers) timer.Dispose();
            _timers.Clear();
            foreach (var function in _modalFunctions)
            {
                _timers.Add(new System.Threading.Timer(async _ =>
                {
                    _navigationRefresh = false;
                    await function(ModalHelper);
                    if (_navigationRefresh) return;
                    _modalFunctions.Remove(function);
                }, null, 10, Timeout.Infinite));
            }
        }
    }

    private async Task OnGlobalModalDispatched(Func<IModalHelper, Task<ModalResult?>> func)
    {
        _modalFunctions.Add(func);
        await func(ModalHelper);
    }
}
