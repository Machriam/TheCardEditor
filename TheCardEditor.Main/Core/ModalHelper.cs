using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Toolbelt.Blazor.HotKeys2;

namespace TheCardEditor.Main.Core;

public interface IModalHelper
{
    void InitializeModalService(IModalService service, IJSRuntime jsRuntime, HotKeys hotkeys);

    Task<TResult?> ShowModal<TModal, TResult>(string title, Dictionary<string, object?> parameter,
            bool moveable = false,
        bool disableBackgroundCancel = false, bool hideCloseButton = false,
         ModalPosition position = ModalPosition.Center) where TModal : IComponent;

    Task<object?> ShowModal<TModal>(string title, Dictionary<string, object?> parameter,
            bool movable = false,
            bool disableBackgroundCancel = false, bool hideCloseButton = false,
            ModalPosition position = ModalPosition.Center) where TModal : IComponent;
}

public class ModalHelper : IModalHelper
{
    private IModalService? _modalService;
    private IJSRuntime? _jsRuntime;
    private HotKeys? _hotkeys;

    public void InitializeModalService(IModalService service, IJSRuntime jsRuntime, HotKeys hotkeys)
    {
        _modalService = service;
        _hotkeys = hotkeys;
        _jsRuntime = jsRuntime;
    }

    public async Task<object?> ShowModal<TModal>(string title, Dictionary<string, object?> parameter,
        bool movable = false,
        bool disableBackgroundCancel = false, bool hideCloseButton = false,
        ModalPosition position = ModalPosition.Center) where TModal : IComponent
    {
        if (_modalService == null || _jsRuntime == null || _hotkeys == null) return null;
        using var hotKeyContext = _hotkeys.CreateContext();
        var parameters = new ModalParameters();
        foreach (var param in parameter) parameters.Add(param.Key, param.Value);
        var options = new ModalOptions()
        {
            DisableBackgroundCancel = disableBackgroundCancel,
            HideCloseButton = hideCloseButton,
            Position = position,
            Class = movable ? "blazored-modal-draggable" : "",
            OverlayCustomClass = movable ? "blazored-disable-overlay" : "",
        };
        var modal = _modalService.Show<TModal>(title, parameters, options);
        if (movable)
        {
            await Task.Delay(1);
            await _jsRuntime.InvokeVoidAsync("window.BlazorModalExtensions.Draggable");
        }
        if (!hideCloseButton) hotKeyContext.Add(Code.Escape, modal.Close);
        return (await modal.Result).Data;
    }

    public async Task<TResult?> ShowModal<TModal, TResult>(string title, Dictionary<string, object?> parameter,
        bool moveable = false,
    bool disableBackgroundCancel = false, bool hideCloseButton = false,
    ModalPosition position = ModalPosition.Center) where TModal : IComponent
    {
        return (TResult?)await ShowModal<TModal>(title, parameter, moveable, disableBackgroundCancel,
            hideCloseButton, position);
    }
}
