using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.JSInterop;

namespace TheCardEditor.Main.Core;

public static class IJsRuntimeExtensions
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
        await jsRuntime.HandledInvokeVoid(path, functionName, parameter.ToArray());
    }

    /// <summary>
    /// Js File to be used must be a razor.js file next to the razor.cs file
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    public static async ValueTask<T?> ExecuteModuleFunction<T>(this IJSRuntime jsRuntime, string functionName,
        IEnumerable<object> parameter, [CallerFilePath] string path = "")
    {
        return await jsRuntime.HandledInvoke<T>(path, functionName, parameter.ToArray());
    }

    /// <summary>
    /// Js File to be imported must be a razor.js file next to the razor.cs file
    /// </summary>
    /// <param name="jsRuntime"></param>
    /// <param name="path"></param>
    /// <returns>The module to invoke Methods on</returns>
    public static ValueTask<IJSObjectReference> ImportJsFile(this IJSRuntime jsRuntime, [CallerFilePath] string path = "")
    {
        var splitText = nameof(TheCardEditor) + "." + nameof(Main);
        var split = path.Split(splitText);
        var modulePath = path;
        if (split.Length == 2) modulePath = "./" + split[^1].Replace(".cs", "") + ".js";
        var buildId = s_buildGuid;
#if DEBUG
        buildId = Guid.NewGuid().ToString();
#endif
        return jsRuntime.InvokeAsync<IJSObjectReference>("import", modulePath + "?version=" + buildId);
    }

    public static async ValueTask<T?> HandledInvoke<T>(this IJSRuntime jsRuntime, string module, string identifier, params object[] args)
    {
        try
        {
            var obj = await jsRuntime.ImportJsFile(module);
            var result = await obj.InvokeAsync<T>(identifier, args);
            await obj.DisposeAsync();
            return result;
        }
        catch (Exception e)
        {
            await jsRuntime.InvokeVoidAsync("alert", e.Message);
            return default;
        }
    }

    public static async ValueTask HandledInvokeVoid(this IJSRuntime jsRuntime, string module, string identifier, params object[] args)
    {
        try
        {
            var jsObject = await jsRuntime.ImportJsFile(module);
            await jsObject.InvokeVoidAsync(identifier, args);
            await jsObject.DisposeAsync();
        }
        catch (Exception e)
        {
            await jsRuntime.InvokeVoidAsync("alert", e.Message);
        }
    }
}
