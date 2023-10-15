using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;
using TheCardEditor.Shared;

namespace TheCardEditor.Main.Core;

public interface IJsInterop : IErrorLogger
{
    Task<IEnumerable<string>> GetAvailableFonts();

    Task LoadFont(string fontName, string base64Data);

    void InvalidateModules();

    Task<string> Prompt(string message);

    Task<bool> Confirm(string message);

    ValueTask ExecuteVoid(Func<IJSRuntime, ValueTask> function);

    ValueTask<T?> Execute<T>(Func<IJSRuntime, ValueTask<T>> function);

    ValueTask ExecuteCodeBehindModule(string functionName, IEnumerable<object> parameter, [CallerFilePath] string path = "");

    ValueTask ExecuteModuleFunction(string path, string functionName, params object[] parameter);

    ValueTask<T?> ExecuteModuleFunction<T>(string path, string functionName, params object[] parameter);

    ValueTask<T?> ExecuteCodeBehindModule<T>(string functionName, IEnumerable<object> parameter, [CallerFilePath] string path = "");
}

public class JsInterop : IJsInterop
{
    private readonly IJSRuntime _jsRuntime;
    private static string s_buildGuid = Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToString();

    public JsInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async ValueTask<T?> Execute<T>(Func<IJSRuntime, ValueTask<T>> function)
    {
        try
        {
            return await function.Invoke(_jsRuntime);
        }
        catch (Exception e)
        {
            await _jsRuntime.InvokeVoidAsync("alert", e.Message);
        }
        return default;
    }

    public async ValueTask ExecuteVoid(Func<IJSRuntime, ValueTask> function)
    {
        try
        {
            await function.Invoke(_jsRuntime);
        }
        catch (Exception e)
        {
            await _jsRuntime.InvokeVoidAsync("alert", e.Message);
        }
    }

    public async ValueTask ExecuteModuleFunction(string path, string functionName, params object[] parameter)
    {
        var jsFile = await ImportJsFile(path);
        if (jsFile == null) return;
        await ExecuteVoid(async _ => await jsFile.InvokeVoidAsync(functionName, parameter));
        await jsFile.DisposeAsync();
    }

    public async ValueTask<T?> ExecuteModuleFunction<T>(string path, string functionName, params object[] parameter)
    {
        var jsFile = await ImportJsFile(path);
        if (jsFile == null) return default;
        var result = await Execute(async _ => await jsFile.InvokeAsync<T>(functionName, parameter));
        await jsFile.DisposeAsync();
        return result;
    }

    /// <summary>
    /// Js File to be used must be a razor.js file next to the razor.cs file
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public async ValueTask ExecuteCodeBehindModule(string functionName, IEnumerable<object> parameter, [CallerFilePath] string path = "")
    {
        await ExecuteModuleFunction(path, functionName, parameter);
    }

    /// <summary>
    /// Js File to be used must be a razor.js file next to the razor.cs file
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public async ValueTask<T?> ExecuteCodeBehindModule<T>(string functionName, IEnumerable<object> parameter, [CallerFilePath] string path = "")
    {
        return await ExecuteModuleFunction<T>(path, functionName, parameter);
    }

    /// <summary>
    /// Js File to be imported must be a razor.js file next to the razor.cs file
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="path"></param>
    /// <returns>The module to invoke Methods on</returns>
    public async ValueTask<IJSObjectReference?> ImportJsFile([CallerFilePath] string path = "")
    {
        const string SplitText = nameof(TheCardEditor) + "." + nameof(Main);
        var split = path.Split(SplitText);
        var modulePath = path;
        if (split.Length == 2) modulePath = "./" + split[^1].Replace(".cs", "") + ".js";
        var buildId = s_buildGuid;
        return await Execute(r => r.InvokeAsync<IJSObjectReference>("import", modulePath + "?version=" + buildId));
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

    public void InvalidateModules()
    {
        s_buildGuid = Guid.NewGuid().ToString();
    }
}
