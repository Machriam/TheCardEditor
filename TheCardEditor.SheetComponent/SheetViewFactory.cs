using Microsoft.JSInterop;

namespace TheCardEditor.SheetComponent;

public interface ISheetViewFactory
{
    IXSheetView CreateSheet<TView>(TView objectReference, string divId) where TView : class;
}

public class SheetViewFactory(IJSRuntime jsRuntime) : ISheetViewFactory
{
    public IXSheetView CreateSheet<TView>(TView objectReference, string divId) where TView : class
    {
        return new XSheetView<TView>(jsRuntime, divId, objectReference);
    }
}
