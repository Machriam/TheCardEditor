using Microsoft.JSInterop;

namespace TheCardEditor.Main.Core.Grid;

public interface IGridView : IDisposable
{
    ValueTask UpdateGrid<TData>(DisplayGridModel<TData> data) where TData : AbstractGridModel;

    ValueTask InsertRow<TData>(TData data, bool before = false) where TData : AbstractGridModel;

    ValueTask<IEnumerable<TData>> GetGridData<TData>() where TData : AbstractGridModel;

    ValueTask SetFilter(GridModelFilter filter);

    ValueTask SelectData(IEnumerable<long> ids);

    ValueTask<IEnumerable<TData>> GetParsedGridData<TData>() where TData : AbstractGridModel;

    ValueTask ExportTSV(string name, bool onlyFilteredRows = false);
}

public class AgGrid<TView> : IGridView where TView : class
{
    private readonly IJSRuntime _jsRuntime;
    private readonly string _divId;
    private bool _initialized;
    private const string Namespace = "genericGridFunctions";
    private static string JsInitialize => Namespace + ".initialize";
    private static string JsUpdate => Namespace + ".updateData";
    private static string JsInsertRow => Namespace + ".insertRow";
    private static string JsDispose => Namespace + ".dispose";
    private static string JsGetData => Namespace + ".getData";
    private static string JsSelectData => Namespace + ".selectData";
    private static string JsApplyFilter => Namespace + ".applyFilter";
    private static string JsGetParsedData => Namespace + ".getParsedData";
    private static string JsCsvExport => Namespace + ".getCsvExport";
    private readonly DotNetObjectReference<TView> _objectReference;
    private readonly string _rowSelectionHandler;
    private readonly bool _multipleRowSelect = false;

    public AgGrid(IJSRuntime jsRuntime, string divId, TView objectReference, string rowSelectionHandler,
        bool multipleRowSelect = false)
    {
        _jsRuntime = jsRuntime;
        _divId = divId;
        _objectReference = DotNetObjectReference.Create(objectReference);
        _rowSelectionHandler = rowSelectionHandler;
        _multipleRowSelect = multipleRowSelect;
    }

    public void Dispose()
    {
        _ = _jsRuntime.InvokeVoidAsync(JsDispose, _divId);
        _objectReference?.Dispose();
    }

    public ValueTask UpdateGrid<TData>(DisplayGridModel<TData> gridModel) where TData : AbstractGridModel
    {
        gridModel.Parameter.RowSelectionHandler = _rowSelectionHandler;
        gridModel.Parameter.MultipleRowSelect = _multipleRowSelect;
        var gridData = gridModel.Data;
        var parameter = gridModel.Parameter;
        if (_initialized) return _jsRuntime.InvokeVoidAsync(JsUpdate, gridData, _divId);
        _initialized = true;
        return _jsRuntime.InvokeVoidAsync(JsInitialize, gridData, parameter, _objectReference, _divId);
    }

    public async ValueTask<IEnumerable<TData>> GetGridData<TData>() where TData : AbstractGridModel
    {
        if (!_initialized) return new List<TData>();
        var data = await _jsRuntime.InvokeAsync<IEnumerable<TData>>(JsGetData, _divId);
        return data ?? new List<TData>();
    }

    public async ValueTask<IEnumerable<TData>> GetParsedGridData<TData>() where TData : AbstractGridModel
    {
        if (!_initialized) return new List<TData>();
        var data = await _jsRuntime.InvokeAsync<IEnumerable<TData>>(JsGetParsedData, _divId);
        return data ?? new List<TData>();
    }

    public async ValueTask ExportTSV(string name, bool onlyFilteredRows = false)
    {
        if (!_initialized) return;
        await _jsRuntime.InvokeVoidAsync(JsCsvExport, name, _divId, onlyFilteredRows);
    }

    public async ValueTask SetFilter(GridModelFilter filter)
    {
        if (!_initialized) return;
        await _jsRuntime.InvokeVoidAsync(JsApplyFilter, filter, _divId);
    }

    public async ValueTask SelectData(IEnumerable<long> ids)
    {
        if (!_initialized) return;
        await _jsRuntime.InvokeVoidAsync(JsSelectData, ids, _divId);
    }

    public async ValueTask InsertRow<TData>(TData data, bool before = false) where TData : AbstractGridModel
    {
        if (!_initialized) return;
        await _jsRuntime.InvokeVoidAsync(JsInsertRow, data, _divId, before);
    }
}
