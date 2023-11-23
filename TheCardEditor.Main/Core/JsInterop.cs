﻿using Microsoft.JSInterop;
using TheCardEditor.Shared;

namespace TheCardEditor.Main.Core;

public interface IJsInterop : IErrorLogger
{
    Task<IEnumerable<string>> GetAvailableFonts();

    Task LoadFont(string fontName, string base64Data);

    Task<string> Prompt(string message);

    Task<bool> Confirm(string message);

    Task ConsoleLog(string message);
}

public class JsInterop : IJsInterop
{
    private readonly IJSRuntime _jsRuntime;

    public JsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task ConsoleLog(string message)
    {
        await _jsRuntime.InvokeVoidAsync("console.log", message);
    }

    public async Task LogError(string message, string stackTrace = "")
    {
        await _jsRuntime.InvokeVoidAsync("console.log", message + "\n" + stackTrace);
        await _jsRuntime.InvokeVoidAsync("alert", message);
    }

    public async Task<bool> Confirm(string message)
    {
        return await _jsRuntime.InvokeAsync<bool>("confirm", message);
    }

    public async Task<string> Prompt(string message)
    {
        return await _jsRuntime.InvokeAsync<string>("prompt", message);
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
