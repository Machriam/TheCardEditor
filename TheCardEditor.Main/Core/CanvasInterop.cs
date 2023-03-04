using Microsoft.JSInterop;

namespace TheCardEditor.Main.Core;

public interface ICanvasInteropFactory
{
    ICanvasInterop CreateCanvas<TView>(TView objectReference, string divId) where TView : class;
}

public class CanvasInteropFactory : ICanvasInteropFactory
{
    private readonly IJSRuntime _jsRuntime;

    public CanvasInteropFactory(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public ICanvasInterop CreateCanvas<TView>(TView objectReference, string divId) where TView : class
    {
        return new CanvasInterop<TView>(_jsRuntime, divId, objectReference);
    }
}

public interface ICanvasInterop : IDisposable
{
    ValueTask DrawPicture(int xPos, int yPos, byte[] image);

    ValueTask DrawText(int xPos, int yPos, string text);

    ValueTask<string> ExportPng();
}

public class CanvasInterop<TView> : ICanvasInterop where TView : class
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string _divId;
    private readonly TView _objectReference;
    private bool _initialized;

    private const string Namespace = "canvasInteropFunctions";
    private static string JsInitialize => Namespace + ".initialize";
    private static string JsDrawText => Namespace + ".drawText";
    private static string JsDrawPicture => Namespace + ".drawPicture";
    private static string JsExport => Namespace + ".exportCanvas";
    private static string JsDispose => Namespace + ".dispose";

    public CanvasInterop(IJSRuntime jsruntime, string divId, TView objectReference)
    {
        _jsRuntime = jsruntime;
        _divId = divId;
        _objectReference = objectReference;
    }

    private async ValueTask Initialize()
    {
        if (_initialized) return;
        await _jsRuntime.HandledInvokeVoid(JsInitialize, _divId);
        _initialized = true;
    }

    public async ValueTask<string> ExportPng()
    {
        await Initialize();
        return await _jsRuntime.HandledInvoke<string>(JsExport, _divId) ?? "";
    }

    public async ValueTask DrawText(int xPos, int yPos, string text)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsDrawText, xPos, yPos, text, _divId);
    }

    public async ValueTask DrawPicture(int xPos, int yPos, byte[] image)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsDrawPicture, xPos, yPos, image, _divId);
    }

    public async void Dispose()
    {
        await _jsRuntime.HandledInvokeVoid(JsDispose, _divId);
    }
}
