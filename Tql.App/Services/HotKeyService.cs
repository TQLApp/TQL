using Tql.App.Interop;

namespace Tql.App.Services;

internal class HotKeyService : IDisposable
{
    private readonly KeyboardHook _keyboardHook;
    private int? _id;

    public event EventHandler? Pressed;

    public HotKeyService(Settings settings)
    {
        _keyboardHook = new KeyboardHook();
        _keyboardHook.KeyPressed += (_, _) => OnPressed();

        RegisterHotKey(HotKey.FromSettings(settings));
    }

    public void RegisterHotKey(HotKey hotKey)
    {
        var originalId = _id;

        var modifierKeys = default(ModifierKeys);
        if (hotKey.Win)
            modifierKeys |= ModifierKeys.Windows;
        if (hotKey.Control)
            modifierKeys |= ModifierKeys.Control;
        if (hotKey.Alt)
            modifierKeys |= ModifierKeys.Alt;
        if (hotKey.Shift)
            modifierKeys |= ModifierKeys.Shift;

        _id = _keyboardHook.RegisterHotKey(modifierKeys, hotKey.Key);

        if (originalId.HasValue)
            _keyboardHook.UnregisterHotKey(originalId.Value);
    }

    protected virtual void OnPressed() => Pressed?.Invoke(this, EventArgs.Empty);

    public void Dispose()
    {
        _keyboardHook.Dispose();
    }
}
