using Microsoft.JSInterop;

namespace TheCardEditor.SheetComponent;

public interface IXSheetView : IDisposable
{
    ValueTask<IEnumerable<TData>?> GetSheetData<TData>() where TData : AbstractSheetModel;

    ValueTask UpdateGrid<TData>(DisplaySheetModel<TData> sheetModel) where TData : AbstractSheetModel;

    ValueTask CopyToClipboard();
}

public class XSheetView<TView>(IJSRuntime jsRuntime, string divId, TView objectReference) : IXSheetView where TView : class
{
    private bool _initialized;
    private const string Namespace = "genericSheetFunctions";
    private const string Initialize = Namespace + ".initialize";
    private const string Update = Namespace + ".update";
    private const string DisposeSheet = Namespace + ".dispose";
    private const string SheetData = Namespace + ".getSheetData";
    private const string CopyDataToClipboard = Namespace + ".copyDataToClipboard";
    private readonly DotNetObjectReference<TView> _objectReference = DotNetObjectReference.Create(objectReference);

    public ValueTask UpdateGrid<TData>(DisplaySheetModel<TData> sheetModel) where TData : AbstractSheetModel
    {
        var gridData = sheetModel.Data.AsJson();
        var parameter = sheetModel.Parameter.AsJson();
        if (_initialized) return jsRuntime.InvokeVoidAsync(Update, gridData, parameter, divId);
        _initialized = true;
        return jsRuntime.InvokeVoidAsync(Initialize, gridData, parameter, divId);
    }

    public async ValueTask<IEnumerable<TData>?> GetSheetData<TData>() where TData : AbstractSheetModel
    {
        var data = await jsRuntime.InvokeAsync<string>(SheetData, divId);
        return data.FromJson<IEnumerable<TData>>(allowNumberReadingFromString: true);
    }

    public async ValueTask CopyToClipboard()
    {
        await jsRuntime.InvokeVoidAsync(CopyDataToClipboard, divId);
    }

    public void Dispose()
    {
        _ = jsRuntime.InvokeVoidAsync(DisposeSheet, divId);
        _objectReference?.Dispose();
    }
}
