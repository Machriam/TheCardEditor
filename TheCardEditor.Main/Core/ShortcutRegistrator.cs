using Toolbelt.Blazor.HotKeys2;

namespace TheCardEditor.Main.Core;

public interface IShortcutRegistrator : IDisposable
{
    ShortcutRegistrator AddHotKey(ModCode modCode, Code code, Func<HotKeyEntryByCode, ValueTask> func, string description);

    ShortcutRegistrator AddHotKey(ModCode modCode, Code code, Func<ValueTask> func, string description);

    IEnumerable<(string KeyCode, string Description)> GetRegisteredHotkeys();
}

public class ShortcutRegistrator : IShortcutRegistrator
{
    private static readonly List<(string Guid, string KeyCode, string Description)> _registeredShortcuts = new();
    private readonly string _guid = Guid.NewGuid().ToString();
    private readonly HotKeys _hotKeys;
    private HotKeysContext? _hotkeyContext;

    public ShortcutRegistrator(HotKeys hotKeys)
    {
        _hotKeys = hotKeys;
    }

    public ShortcutRegistrator AddHotKey(ModCode modCode, Code code, Func<ValueTask> func, string description)
    {
        _hotkeyContext ??= _hotKeys.CreateContext();
        _hotkeyContext.Add(modCode, code, func);
        _registeredShortcuts.Add((_guid, (modCode == ModCode.None ? "" : Enum.GetName(modCode) + "+") + code.ToString(), description));
        return this;
    }

    public ShortcutRegistrator AddHotKey(ModCode modCode, Code code, Func<HotKeyEntryByCode, ValueTask> func, string description)
    {
        _hotkeyContext ??= _hotKeys.CreateContext();
        _hotkeyContext.Add(modCode, code, func);
        _registeredShortcuts.Add((_guid, (modCode == ModCode.None ? "" : Enum.GetName(modCode) + "+") + code.ToString(), description));
        return this;
    }

    public IEnumerable<(string KeyCode, string Description)> GetRegisteredHotkeys()
    {
        return _registeredShortcuts.Select(r => (r.KeyCode, r.Description));
    }

    public void Dispose()
    {
        _registeredShortcuts.RemoveAll(rs => rs.Guid == _guid);
        _hotkeyContext?.Dispose();
    }
}
