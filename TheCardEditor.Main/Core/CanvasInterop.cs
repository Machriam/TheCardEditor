﻿using System.ComponentModel;
using System.Text.Json.Nodes;
using Microsoft.JSInterop;
using TheCardEditor.Shared;
using TheCardEditor.Shared.Features.CardEditor;
using Toolbelt.Blazor.HotKeys2;

namespace TheCardEditor.Main.Core;

public struct ObjectParameter
{
    public ObjectParameter()
    {
    }

    public long PictureId { get; set; }
    public float Left { get; set; }
    public float Top { get; set; }
    public float Angle { get; set; }
    public string? Tag { get; set; } = "";
    public int? TextSize { get; set; }
}

public delegate void SelectCanvasObjectHandler(ObjectParameter param);

public delegate Task SelectCanvasObjectHandlerAsync(ObjectParameter param);

public enum CanvasFontStyle
{
    [Description("stroke")]
    Stroke,

    [Description("fill")]
    Fill,

    [Description("fontFamily")]
    FontFamily,

    [Description("fontSize")]
    FontSize,

    [Description("fontWeight")]
    FontWeight,

    [Description("textAlign")]
    TextAlign,

    [Description("fontStyle")]
    FontStyle,

    [Description("underline")]
    Underline,

    [Description("overline")]
    Overline,

    [Description("linethrough")]
    Linethrough,

    [Description("clear")]
    Clear
}

public interface ICanvasInteropFactory
{
    ICanvasInterop CreateCanvas<TView>(TView objectReference, string divId,
        SelectCanvasObjectHandlerAsync objectSelectionHandler, Func<Task> objectDeselectionHandler,
        Func<Task> multiObjectSelectionHandler) where TView : class;

    ICanvasInterop CreateCanvas<TView>(TView objectReference, string divId,
    SelectCanvasObjectHandler objectSelectionHandler, Action objectDeselectionHandler,
    Action multiObjectSelectionHandler) where TView : class;
}

public class CanvasParameter
{
    public string ObjectSelectionHandler { get; set; } = "";
    public string ObjectDeselectionHandler { get; set; } = "";
    public string MultiObjectSelectionHandler { get; set; } = "";
    public object DotnetReference { get; set; } = new();
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

    public ICanvasInterop CreateCanvas<TView>(TView objectReference, string divId,
        SelectCanvasObjectHandlerAsync objectSelectionHandler, Func<Task> objectDeselectionHandler,
        Func<Task> multiObjectSelectionHandler) where TView : class
    {
        return new CanvasInterop<TView>(_jsRuntime, divId, objectReference, _hotKeys, objectSelectionHandler.Method.Name,
            objectDeselectionHandler.Method.Name, multiObjectSelectionHandler.Method.Name);
    }

    public ICanvasInterop CreateCanvas<TView>(TView objectReference, string divId,
    SelectCanvasObjectHandler objectSelectionHandler, Action objectDeselectionHandler,
    Action multiObjectSelectionHandler) where TView : class
    {
        return new CanvasInterop<TView>(_jsRuntime, divId, objectReference, _hotKeys, objectSelectionHandler.Method.Name,
            objectDeselectionHandler.Method.Name, multiObjectSelectionHandler.Method.Name);
    }
}

public interface ICanvasInterop : IDisposable
{
    ValueTask<string> ExportPng();

    ValueTask<JsonObject> ExportJson();

    ValueTask SetCoordinates(int left, int top, decimal angle);

    ValueTask DrawPicture(int xPos, int yPos, long id, string name, string base64Image);

    ValueTask DrawText(int xPos, int yPos, string text, string tag, int fontSize);

    ValueTask SelectObject(int index);

    ValueTask CenterObjects();

    ValueTask ApplyFont(CanvasFontStyle style, object value);

    ValueTask<int> SendBackwards(int index);

    ValueTask<int> BringForward(int index);

    ValueTask RemoveObject();

    ValueTask Zoom(double zoom);

    ValueTask ImportJson(JsonObject json, Dictionary<long, string> imageData);

    ValueTask Reset();

    ValueTask<ObjectParameter> GetObjectParameter();

    ValueTask UpdateImage(string base64Image, ImageFilterPipeline filterPipeline);
}

public class CanvasInterop<TView> : ICanvasInterop where TView : class
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string _divId;
    private readonly DotNetObjectReference<TView> _objectReference;
    private readonly HotKeys _hotKeys;
    private readonly string _selectionHandlerName;
    private readonly string _deselectionHandlerName;
    private readonly string _multiObjectSelectedHandler;
    private HotKeysContext? _hotKeysContext;
    private bool _initialized;

    private const string Namespace = "canvasInteropFunctions";
    private static string JsInitialize => Namespace + ".initialize";
    private static string JsDrawText => Namespace + ".drawText";
    private static string JsRemoveObject => Namespace + ".removeObject";
    private static string JsDrawPicture => Namespace + ".drawPicture";
    private static string JsExport => Namespace + ".exportCanvas";
    private static string JsSelectObject => Namespace + ".selectObject";
    private static string JsApplyFont => Namespace + ".applyFont";
    private static string JsJsonExport => Namespace + ".exportJson";
    private static string JsSetCoordinates => Namespace + ".setCoordinates";
    private static string JsBringForward => Namespace + ".bringForward";
    private static string JsSendBackwards => Namespace + ".sendBackwards";
    private static string JsJsonImport => Namespace + ".importJson";
    private static string OnKeyDown => Namespace + ".onKeyDown";
    private static string JsDispose => Namespace + ".dispose";
    private static string JsZoom => Namespace + ".zoom";
    private static string JsCenterObjects => Namespace + ".centerObjects";
    private static string JsReset => Namespace + ".reset";
    private static string JsGetObjectParameter => Namespace + ".getObjectParameter";
    private static string JsUpdateImage => Namespace + ".updateImage";

    public CanvasInterop(IJSRuntime jsruntime, string divId, TView objectReference, HotKeys hotKeys, string selectionHandlerName, string deselectionHandlerName,
                         string multiObjectSelectedHandler)
    {
        _jsRuntime = jsruntime;
        _divId = divId;
        _objectReference = DotNetObjectReference.Create(objectReference);
        _hotKeys = hotKeys;
        _selectionHandlerName = selectionHandlerName;
        _deselectionHandlerName = deselectionHandlerName;
        _multiObjectSelectedHandler = multiObjectSelectedHandler;
    }

    private async ValueTask Initialize()
    {
        if (_initialized) return;
        _hotKeys.KeyDown += HotKeys_KeyDown;
        _hotKeysContext = _hotKeys.CreateContext();
        await _jsRuntime.HandledInvokeVoid(JsInitialize, _divId, new CanvasParameter()
        {
            ObjectDeselectionHandler = _deselectionHandlerName,
            ObjectSelectionHandler = _selectionHandlerName,
            MultiObjectSelectionHandler = _multiObjectSelectedHandler,
            DotnetReference = _objectReference
        });
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

    public async ValueTask<ObjectParameter> GetObjectParameter()
    {
        await Initialize();
        return await _jsRuntime.HandledInvoke<ObjectParameter>(JsGetObjectParameter, _divId);
    }

    public async ValueTask UpdateImage(string base64Image, ImageFilterPipeline filterPipeline)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsUpdateImage, base64Image, filterPipeline, _divId);
    }

    public async ValueTask<int> SendBackwards(int index)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsSendBackwards, index, _divId);
        return index == 0 ? 0 : index - 1;
    }

    public async ValueTask<int> BringForward(int index)
    {
        await Initialize();
        return await _jsRuntime.HandledInvoke<int>(JsBringForward, index, _divId);
    }

    public async ValueTask<string> ExportPng()
    {
        await Initialize();
        return await _jsRuntime.HandledInvoke<string>(JsExport, _divId) ?? "";
    }

    public async ValueTask DrawText(int xPos, int yPos, string text, string tag, int fontSize)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsDrawText, xPos, yPos, text, tag, fontSize, _divId);
    }

    public async ValueTask RemoveObject()
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsRemoveObject, _divId);
    }

    public async ValueTask DrawPicture(int xPos, int yPos, long id, string name, string base64Image)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsDrawPicture, xPos, yPos, id, name, base64Image, _divId);
    }

    public async ValueTask ImportJson(JsonObject json, Dictionary<long, string> imageData)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsJsonImport, json, imageData, _divId);
    }

    public async ValueTask<JsonObject> ExportJson()
    {
        await Initialize();
        return await _jsRuntime.HandledInvoke<JsonObject>(JsJsonExport, _divId) ?? throw new Exception("JsonExport method not found");
    }

    public async void Dispose()
    {
        await _jsRuntime.HandledInvokeVoid(JsDispose, _divId);
        _hotKeysContext?.Dispose();
    }

    public async ValueTask SetCoordinates(int left, int top, decimal angle)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsSetCoordinates, _divId, left, top, angle);
    }

    public async ValueTask CenterObjects()
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsCenterObjects, _divId);
    }

    public async ValueTask Reset()
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsReset, _divId);
    }

    public async ValueTask Zoom(double zoom)
    {
        await Initialize();
        await _jsRuntime.HandledInvokeVoid(JsZoom, zoom, _divId);
    }
}
