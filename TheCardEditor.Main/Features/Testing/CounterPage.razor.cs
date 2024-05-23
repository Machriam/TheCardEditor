using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using TheCardEditor.Main.Core;
using TheCardEditor.SheetComponent;

namespace TheCardEditor.Main.Features.Testing;

public partial class CounterPage : IDisposable
{
    private class SheetDataModel : AbstractSheetModel
    {
        [SheetMetaData(HeaderName = "First Column")]
        public string FirstColumn { get; set; } = Guid.NewGuid().ToString();

        [SheetMetaData(HeaderName = "Number Test", SheetConverter = SheetConverter.Numeric)]
        public int Second { get; set; } = Random.Shared.Next(1000);
    }

    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ICanvasInteropFactory CanvasInteropFactory { get; set; } = default!;
    [Inject] private ISheetViewFactory SheetViewFactory { get; set; } = default!;
    private IXSheetView _sheetView = default!;
    private const string SheetviewGridId = nameof(SheetviewGridId);
    private ICanvasInterop _canvasInterop = default!;
    public const string Location = "Counter";
    private JsonObject? _json;
    private IEnumerable<(string? Name, string? Tag, int Index)>? _canvasObjects;
    private const string CanvasId = "MySweetCanvas";
    private int currentCount = 0;
    private int FontSize { get; set; } = 12;
    private int SelectIndex { get; set; } = 0;
    private string selectedFont = "";
    private List<string> AvailableFonts = new();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender) return;
        _sheetView = SheetViewFactory.CreateSheet(this, SheetviewGridId);
        await _sheetView.UpdateGrid(new DisplaySheetModel<SheetDataModel>(Enumerable.Range(0, 100).Select(_ => new SheetDataModel())));
        AvailableFonts.AddRange(await JS.GetAvailableFonts());
        _canvasInterop = CanvasInteropFactory.CreateCanvas(this, CanvasId, ObjectSelected, ObjectDeselected, MultiObjectSelected);
        StateHasChanged();
    }

    private async Task LoadFont()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "ShittyFont.woff2");
        await JS.LoadFont("Shitty Test Font", Convert.ToBase64String(File.ReadAllBytes(path)));
        AvailableFonts.Add("Shitty Test Font");
        StateHasChanged();
    }

    private async Task LoadJson()
    {
        if (_json == null) return;
        await _canvasInterop.ImportJson(_json, new());
    }

    private async Task SelectObject()
    {
        await _canvasInterop.SelectObject(SelectIndex);
    }

    private async Task ApplyFont(CanvasFontStyle style, object value)
    {
        await _canvasInterop.ApplyFont(style, value);
    }

    private IEnumerable<(string? Name, string? Tag, int Index)> GetObjects()
    {
        var result = new List<(string?, string?, int)>();
        if (_json == null) return result;
        var jsonArray = (JsonArray)_json["objects"]!;
        for (var i = 0; i < jsonArray.Count; i++)
        {
            result.Add((jsonArray[i]?["name"]?.GetValue<string>(), jsonArray[i]?["tag"]?.GetValue<string>(), i));
        }
        return result;
    }

    private async Task BringForward()
    {
        SelectIndex = await _canvasInterop.BringForward(SelectIndex);
    }

    private async Task SendBackwards()
    {
        SelectIndex = await _canvasInterop.SendBackwards(SelectIndex);
    }

    private async Task SaveJson()
    {
        _json = await _canvasInterop.ExportJson();
        _canvasObjects = GetObjects();
        StateHasChanged();
    }

    private async Task AddPicture()
    {
        currentCount++;
        await _canvasInterop.DrawPicture(10, 20, 1, "name", "data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAA4AAAASCAIAAADkPnhmAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAIxSURBVDhPJVLbctMwEJVWsiTHl6RJyrTAYwktD53hYxj4e+Cx0DjxTdZdrIsfPJZ1dvfsOYf++P6N0FRtNkqVhGQheF2VTV3VdSll0bbV3d1t111+/voNdaXwDv9yTkXBi4ID0BDCYqyPKcRsjI0pciFgt2uO7/bNttrU6ni7223rnNOsdd+PQiqh1OvrX2vt/rAHrW2MlHOlVMV5oaTc79ptUyrJcgzeuhQzIYwxAdO0mMXHSCjlhACi26ZCVlKwFLy3NmeSUg4+soeHT84645zzniFQFIxBiDHlCID1mVHQi3k9X6EoS2PtOEyXru+HeZqNcSETYIXAwTnnQvCU0zjOcLi/p0LgtuOoz11/uY6LdgiVagNMoCJ1U0mpcgJo2+bpy+P+cIM7mMUM49z1U8ooYhNCHvrlcpn7fp5nDWVRnD4/HA43yB4Jz/PSXWfnc1EoY3x3GV9erudumLRmT0+P0zh257N3jpI3dpnE4KZpvPaDdQ7t0Is11rHT6bQsxnlHEZgTviihaA+CYoxY532weHAeaM6oDG7MpOCrs6gXvH1IITaMSx+SxU7OAZq+jswZKBVy9ZJhCmCl4rzRGj3WBgVEKF4jjsQIKZVSqFICo6g8yUHrcRiu8zShMsiYPX99fv/hY4XhWocyzpgS/70tjVmcRYpAUB3sRSlgOpq2rqoNBhEJ7rbNtl2hDABTxoCtaEKhqTeX8x89DSiQdx4DcDzeYBmCcCg+GclgQ4B/wx9tcrPlIgoAAAAASUVORK5CYII=");
    }

    private async Task IncrementCount()
    {
        currentCount++;
        await _canvasInterop.DrawText(10, 20, currentCount.ToString() + "\n" + "asdf", "new Tag", 999);
        var png = await _canvasInterop.ExportPng();
    }

    public void Dispose()
    {
        _canvasInterop?.Dispose();
        _sheetView?.Dispose();
    }

    [JSInvokable]
    public void ObjectDeselected()
    {
    }

    [JSInvokable]
    public void MultiObjectSelected()
    {
    }

    [JSInvokable]
    public void ObjectSelected(ObjectParameter param)
    {
    }
}
