using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Shared;
using Toolbelt.Blazor.HotKeys2;

namespace TheCardEditor.Main.Core;

public interface IModalHelper
{
    event Func<Func<IModalHelper, Task<ModalResult?>>, Task>? DispatchGlobalModal;

    void InitializeModalService(IModalService service, IJSRuntime jsRuntime, HotKeys hotkeys);

    Task<TResult?> ShowModal<TModal, TResult>(string title, Dictionary<string, object?> parameter,
        bool moveable = false,
        Guid? guid = null,
        bool disableBackgroundCancel = false, bool hideCloseButton = false,
         ModalPosition position = ModalPosition.Middle) where TModal : IComponent;

    Task<ModalResult?> ShowModal<TModal>(string title, Dictionary<string, object?> parameter,
            bool movable = false,
            Guid? guid = null,
            bool disableBackgroundCancel = false, bool hideCloseButton = false,
            ModalPosition position = ModalPosition.Middle) where TModal : IComponent;

    void AddGlobalModalWindow(Func<IModalHelper, Guid, Task<ModalResult?>> showModalFunction);
}

public class ModalHelper : IModalHelper
{
    private record struct ModalUiPosition(int Top, int Left);
    private IModalService? _modalService;
    private IJSRuntime? _jsRuntime;
    private HotKeys? _hotkeys;
    private static readonly Dictionary<Guid, ModalUiPosition> s_modalPositionByGuid = [];
    public const string ModalGuid = nameof(ModalGuid);

    public event Func<Func<IModalHelper, Task<ModalResult?>>, Task>? DispatchGlobalModal;

    public void AddGlobalModalWindow(Func<IModalHelper, Guid, Task<ModalResult?>> showModalFunction)
    {
        var guid = Guid.NewGuid();
        s_modalPositionByGuid.Add(guid, new(100, 100));
        Task<ModalResult?> FunctionWithGuid(IModalHelper modalHelper) => showModalFunction(modalHelper, guid);
        DispatchGlobalModal?.Invoke(FunctionWithGuid);
    }

    [JSInvokable]
    public void ModalPositionChanged(string guid, int newTop, int newLeft)
    {
        s_modalPositionByGuid[Guid.Parse(guid)] = new(newTop, newLeft);
    }

    public void InitializeModalService(IModalService service, IJSRuntime jsRuntime, HotKeys hotkeys)
    {
        _modalService = service;
        _hotkeys = hotkeys;
        _jsRuntime = jsRuntime;
    }

    public async Task<ModalResult?> ShowModal<TModal>(string title, Dictionary<string, object?> parameter,
        bool movable = false,
        Guid? guid = null,
        bool disableBackgroundCancel = false, bool hideCloseButton = false,
        ModalPosition position = ModalPosition.Middle) where TModal : IComponent
    {
        if (_modalService == null || _jsRuntime == null || _hotkeys == null) return null;
        using var hotKeyContext = _hotkeys.CreateContext();
        var parameters = new ModalParameters();
        foreach (var param in parameter) parameters.Add(param.Key, param.Value!);
        var options = new ModalOptions()
        {
            DisableBackgroundCancel = disableBackgroundCancel,
            HideCloseButton = hideCloseButton,
            Size = ModalSize.Automatic,
            Position = position,
            Class = movable ? $"blazored-modal-draggable {guid}" : "",
            OverlayCustomClass = movable ? "blazored-disable-overlay" : "",
        };
        var modal = _modalService.Show<TModal>(title, parameters, options);
        if (movable)
        {
            await Task.Delay(1);
            var modalWindow = new ModalUiPosition(50, 0);
            if (guid != null) s_modalPositionByGuid.TryGetValue(guid.Value, out modalWindow);
            await _jsRuntime.InvokeVoidAsync("window.BlazorModalExtensions.Draggable",
                DotNetObjectReference.Create(this), guid?.ToString(), nameof(ModalPositionChanged),
                modalWindow.Top, modalWindow.Left);
        }
        if (!hideCloseButton) hotKeyContext.Add(Code.Escape, modal.Close);
        return await modal.Result;
    }

    public async Task<TResult?> ShowModal<TModal, TResult>(string title, Dictionary<string, object?> parameter,
        bool moveable = false,
        Guid? guid = null,
    bool disableBackgroundCancel = false, bool hideCloseButton = false,
    ModalPosition position = ModalPosition.Middle) where TModal : IComponent
    {
        return (TResult?)(await ShowModal<TModal>(title, parameter, moveable, guid, disableBackgroundCancel,
            hideCloseButton, position))?.Data;
    }
}
