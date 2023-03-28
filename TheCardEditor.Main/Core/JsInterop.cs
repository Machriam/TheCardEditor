using Microsoft.JSInterop;
using TheCardEditor.Shared;

namespace TheCardEditor.Main.Core;

public interface IJsInterop : IErrorLogger
{
    Task<IEnumerable<string>> GetAvailableFonts();
    Task LoadFont(string fontName, string base64Data);
}

public class JsInterop : IJsInterop
{
    private readonly IJSRuntime _jsRuntime;

    public JsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task LogError(string message, string stackTrace = "")
    {
        await _jsRuntime.InvokeVoidAsync("console.log", message + "\n" + stackTrace);
        await _jsRuntime.InvokeVoidAsync("alert", message);
    }

    public async Task<IEnumerable<string>> GetAvailableFonts()
    {
        return await _jsRuntime.InvokeAsync<IEnumerable<string>>("coreFunctions.getAvailableFonts");
    }

    public async Task LoadFont(string fontName, string base64Data)
    {
        await _jsRuntime.InvokeVoidAsync("coreFunctions.loadFont", fontName, base64Data);
    }
}
