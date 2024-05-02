using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;
using Toolbelt.Blazor.HotKeys2;

namespace TheCardEditor.Main.Features.AppLayout;

public partial class MainLayout
{
    [Inject] private IModalHelper ModalHelper { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private HotKeys HotKeys { get; set; } = default!;
    [CascadingParameter] private IModalService ModalService { get; set; } = default!;

    protected override void OnInitialized()
    {
        ModalHelper.InitializeModalService(ModalService, JSRuntime, HotKeys);
    }
}
