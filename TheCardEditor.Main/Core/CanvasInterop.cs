using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.JSInterop;
using TheCardEditor.Shared;
using Toolbelt.Blazor.HotKeys2;

namespace TheCardEditor.Main.Core;

public enum CanvasFontStyle
{
    [Description("stroke")]
    Stroke,

    [Description("fontFamily")]
    FontFamily,

    [Description("fontSize")]
    FontSize,

    [Description("fontWeight")]
    FontWeight,

    [Description("fontStyle")]
    FontStyle,

    [Description("underline")]
    Underline,

    [Description("overline")]
    Overline,

    [Description("linethrough")]
    Linethrough
}

public interface ICanvasInteropFactory
{
    ICanvasInterop CreateCanvas<TView>(TView objectReference, string divId) where TView : class;
}

public class CanvasInteropFactory : ICanvasInteropFactory
{
    private readonly IJSRuntime _jsRuntime;
    private readonly HotKeys _hotKeys;

    public CanvasInteropFactory(IJSRuntime jsRuntime, HotKeys hotKeys)
    {
        _jsRuntime = jsRuntime;
        _hotKeys = hotKeys;
    }

    public ICanvasInterop CreateCanvas<TView>(TView objectReference, string divId) where TView : class
    {
        return new CanvasInterop<TView>(_jsRuntime, divId, objectReference, _hotKeys);
    }
}

public interface ICanvasInterop : IDisposable
{
    ValueTask<string> ExportPng();

    ValueTask<JsonObject> ExportJson();

    ValueTask ImportJson(JsonObject json);

    ValueTask DrawPicture(int xPos, int yPos, long id, string name, string base64Image);

    ValueTask DrawText(int xPos, int yPos, string text, string tag);

    ValueTask SelectObject(int index);

    ValueTask ApplyFont(CanvasFontStyle style, object value);
}

public class CanvasInterop<TView> : ICanvasInterop where TView : class
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string _divId;
    private readonly TView _objectReference;
    private readonly HotKeys _hotKeys;
    private HotKeysContext? _hotKeysContext;
    private bool _initialized;

    private const string Namespace = "canvasInteropFunctions";
    private static string JsInitialize => Namespace + ".initialize";
    private static string JsDrawText => Namespace + ".drawText";
    private static string JsDrawPicture => Namespace + ".drawPicture";
    private static string JsExport => Namespace + ".exportCanvas";
    private static string JsSelectObject => Namespace + ".selectObject";
    private static string JsApplyFont => Namespace + ".applyFont";
    private static string JsonExport => Namespace + ".exportJson";
    private static string JsonImport => Namespace + ".importJson";
    private static string OnKeyDown => Namespace + ".onKeyDown";
    private static string JsDispose => Namespace + ".dispose";

    public CanvasInterop(IJSRuntime jsruntime, string divId, TView objectReference, HotKeys hotKeys)
    {
        _jsRuntime = jsruntime;
        _divId = divId;
        _objectReference = objectReference;
        _hotKeys = hotKeys;
    }

    private async ValueTask Initialize()
    {
        if (_initialized) return;
        _hotKeys.KeyDown += HotKeys_KeyDown;
        _hotKeysContext = _hotKeys.CreateContext();
        await _jsRuntime.HandledInvokeVoid(JsInitialize, _divId);
        _initialized = true;
    }

    private async void HotKeys_KeyDown(object? sender, HotKeyDownEventArgs e)
    {
        await _jsRuntime.HandledInvokeVoid(OnKeyDown, e.Key, _divId);
    }

    public async ValueTask ApplyFont(CanvasFontStyle style, object value)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsApplyFont, style.GetDescription(), value, _divId);
    }

    public async ValueTask SelectObject(int index)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsSelectObject, index, _divId);
    }

    public async ValueTask<string> ExportPng()
    {
        await Initialize();
        return await _jsRuntime.HandledInvoke<string>(JsExport, _divId) ?? "";
    }

    public async ValueTask DrawText(int xPos, int yPos, string text, string tag)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsDrawText, xPos, yPos, text, tag, _divId);
    }

    public async ValueTask DrawPicture(int xPos, int yPos, long id, string name, string base64Image)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsDrawPicture, xPos, yPos, id, name, base64Image, _divId);
    }

    public async ValueTask ImportJson(JsonObject json)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsonImport, json, _divId);
    }

    public async ValueTask<JsonObject> ExportJson()
    {
        await Initialize();
        return await _jsRuntime.HandledInvoke<JsonObject>(JsonExport, _divId) ?? throw new Exception("JsonExport method not found");
    }

    public async void Dispose()
    {
        await _jsRuntime.HandledInvokeVoid(JsDispose, _divId);
        _hotKeysContext?.Dispose();
    }
}
