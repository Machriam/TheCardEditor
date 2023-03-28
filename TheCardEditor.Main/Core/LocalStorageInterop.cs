using Microsoft.JSInterop;

namespace TheCardEditor.Main.Core;

public interface ILocalStorageInterop
{
    Task DeleteItem(string key);

    Task<string> GetItem(string key);

    Task SetItem(string key, string value);
}

public class LocalStorageInterop : ILocalStorageInterop
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task SetItem(string key, string value)
    {
        await _jsRuntime.InvokeVoidAsync("window.localStorage.setItem", key, value);
    }

    public async Task<string> GetItem(string key)
    {
        return await _jsRuntime.InvokeAsync<string>("window.localStorage.getItem", key);
    }

    public async Task DeleteItem(string key)
    {
        await _jsRuntime.InvokeAsync<string>("window.localStorage.removeItem", key);
    }
}
