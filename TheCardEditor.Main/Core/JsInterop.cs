using Microsoft.JSInterop;

namespace TheCardEditor;

public interface IErrorLogger
{
    Task LogError(string message, string stackTrace = "");
}

public class JsInterop : IErrorLogger
{
    private readonly IJSRuntime _jsRuntime;

    public JsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task LogError(string message, string stackTrace = "")
    {
        await _jsRuntime.InvokeVoidAsync("alert", message + "\n" + stackTrace);
    }
}
