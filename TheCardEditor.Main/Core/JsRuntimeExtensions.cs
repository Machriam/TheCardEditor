using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.JSInterop;

namespace TheCardEditor.Main.Core;

public static partial class JsRuntimeExtensions
{
    private static readonly string s_buildGuid = Assembly.GetExecutingAssembly().ManifestModule.ModuleVersionId.ToString();

    /// <summary>
    /// Js File to be used must be a razor.js file next to the razor.cs file
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async ValueTask ExecuteModuleFunction(this IJSRuntime jsRuntime, string functionName,
        IEnumerable<object> parameter, [CallerFilePath] string path = "")
    {
        var module = await ImportJsFile(jsRuntime, path);
        await module.InvokeVoidAsync(functionName, parameter.ToArray());
        await module.DisposeAsync();
    }

    /// <summary>
    /// Js File to be used must be a razor.js file next to the razor.cs file
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async ValueTask<T> ExecuteModuleFunction<T>(this IJSRuntime jsRuntime, string functionName,
        IEnumerable<object> parameter, [CallerFilePath] string path = "")
    {
        var module = await ImportJsFile(jsRuntime, path);
        var result = await module.InvokeAsync<T>(functionName, parameter.ToArray());
        await module.DisposeAsync();
        return result;
    }

    /// <summary>
    /// Js File to be imported must be a razor.js file next to the razor.cs file
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async ValueTask<IJSObjectReference> ImportJsFile(this IJSRuntime jsRuntime, [CallerFilePath] string path = "")
    {
        var regex = FilePathSplitRegex();
        var split = regex.Split(path);
        var modulePath = path;
        if (split.Length == 2) modulePath = "./" + split[^1].Replace(".cs", "") + ".js";
        var buildId = s_buildGuid;
#if DEBUG
        buildId = Guid.NewGuid().ToString();
#endif
        return await jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath + "?version=" + buildId);
    }

    public static ValueTask<bool> Confirm(this IJSRuntime jsRuntime, string message)
    {
        return jsRuntime.InvokeAsync<bool>("confirm", message);
    }

    public static async ValueTask<bool> ConfirmDeletion(this IJSRuntime jsRuntime, string item)
    {
        return await jsRuntime.InvokeAsync<string>("prompt", $"If you really want to delete: '{item}' then enter 'yes'")
             == "yes";
    }

    public static async ValueTask<string> GetUserString(this IJSRuntime jsRuntime, string question, string defaultMessage = "")
    {
        return await jsRuntime.InvokeAsync<string>("prompt", question, defaultMessage);
    }

    public static async Task Prompt(this IJSRuntime jsRuntime, string message)
    {
        await jsRuntime.InvokeVoidAsync("alert", message);
    }

    public static async Task ConsoleLog(this IJSRuntime jsRuntime, string message)
    {
        await jsRuntime.InvokeVoidAsync("console.log", message);
    }

    [GeneratedRegex(nameof(TheCardEditor) + "." + nameof(Main))]
    private static partial Regex FilePathSplitRegex();

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

    public static async Task LogError(this IJSRuntime jsRuntime, string message, string stackTrace = "")
    {
        await jsRuntime.InvokeVoidAsync("console.log", message + "\n" + stackTrace);
        await jsRuntime.InvokeVoidAsync("alert", message);
    }

    public static async Task<IEnumerable<string>> GetAvailableFonts(this IJSRuntime jsRuntime)
    {
        return await jsRuntime.InvokeAsync<IEnumerable<string>>("coreFunctions.getAvailableFonts");
    }

    public static async Task LoadFont(this IJSRuntime jsRuntime, string fontName, string base64Data)
    {
        await jsRuntime.InvokeVoidAsync("coreFunctions.loadFont", fontName, base64Data);
    }
}
