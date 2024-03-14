using TheCardEditor.Shared;

namespace TheCardEditor.Main.Core;

using Microsoft.JSInterop;

public class ErrorLogger(IJSRuntime jsRuntime) : IErrorLogger
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;

    public async Task LogError(string message, string stackTrace = "") => await _jsRuntime.LogError(message, stackTrace);
}
