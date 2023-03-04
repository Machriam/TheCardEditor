using Microsoft.JSInterop;

namespace TheCardEditor.Main.Core;

public static class IJsRuntimeExtensions
{
    public static async ValueTask<T?> HandledInvoke<T>(this IJSRuntime jsRuntime, string identifier, params object[] args)
    {
        try
        {
            return await jsRuntime.InvokeAsync<T>(identifier, args);
        }
        catch (Exception e)
        {
            await jsRuntime.InvokeVoidAsync("alert", e.Message);
            return default;
        }
    }

    public static async ValueTask HandledInvokeVoid(this IJSRuntime jsRuntime, string identifier, params object[] args)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync(identifier, args);
        }
        catch (Exception e)
        {
            await jsRuntime.InvokeVoidAsync("alert", e.Message);
        }
    }
}
