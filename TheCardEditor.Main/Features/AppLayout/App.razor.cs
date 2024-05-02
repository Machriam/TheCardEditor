using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;

namespace TheCardEditor.Main.Features.AppLayout;

public partial class App
{
    [Inject] private IModalHelper ModalHelper { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    private Func<IModalHelper, Task<ModalResult?>>? _modalFunction;
    private System.Threading.Timer? _timer;
    private bool _navigationRefresh;

    protected override void OnInitialized()
    {
        NavigationManager.LocationChanged += NavigationManager_LocationChanged;
        ModalHelper.DispatchGlobalModal += OnGlobalModalDispatched;
    }

    private void NavigationManager_LocationChanged(object? sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e)
    {
        _navigationRefresh = true;
        if (_modalFunction != null)
        {
            _timer?.Dispose();
            _timer = new System.Threading.Timer(async _ =>
            {
                _navigationRefresh = false;
                await _modalFunction(ModalHelper);
                if (_navigationRefresh) return;
                _modalFunction = null;
            }, null, 10, Timeout.Infinite);
        }
    }

    private async Task OnGlobalModalDispatched(Func<IModalHelper, Task<ModalResult?>> func)
    {
        _modalFunction = func;
        await func(ModalHelper);
    }
}
