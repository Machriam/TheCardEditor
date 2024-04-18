﻿using Microsoft.JSInterop;

namespace TheCardEditor.Main.Core.Grid;

public delegate Task SelectedRowHandlerAsync(long id);

public delegate Task SelectedRowsHandlerAsync(long[] id);

public delegate void SelectedRowHandler(long id);

public delegate void SelectedRowsHandler(long[] id);

public interface IGridViewFactory
{
    IGridView CreateGrid<TView>(TView objectReference, string divId, SelectedRowHandler rowHandler) where TView : class;

    IGridView CreateGrid<TView>(TView objectReference, string divId, SelectedRowsHandler rowHandler) where TView : class;
}

public class GridViewFactory : IGridViewFactory
{
    private readonly IJSRuntime _jsRuntime;

    public GridViewFactory(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public IGridView CreateGrid<TView>(TView objectReference, string divId, SelectedRowHandler rowHandler) where TView : class
    {
        return new AgGrid<TView>(_jsRuntime, divId, objectReference, rowHandler.Method.Name);
    }

    public IGridView CreateGrid<TView>(TView objectReference, string divId, SelectedRowsHandler rowHandler) where TView : class
    {
        return new AgGrid<TView>(_jsRuntime, divId, objectReference, rowHandler.Method.Name, multipleRowSelect: true);
    }
}
