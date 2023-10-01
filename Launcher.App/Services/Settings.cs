using Microsoft.Win32;

namespace Launcher.App.Services;

internal class Settings
{
    public const int DefaultHistoryInRootResults = 90;

    private readonly RegistryKey _key;

    public int? ShowOnScreen
    {
        get => GetInteger(nameof(ShowOnScreen));
        set => SetInteger(nameof(ShowOnScreen), value);
    }

    public int? HistoryInRootResults
    {
        get => GetInteger(nameof(HistoryInRootResults));
        set => SetInteger(nameof(HistoryInRootResults), value);
    }

    public Settings(IStore store)
    {
        _key = store.CreateBaseKey();
    }

    private string? GetString(string name) => _key.GetValue(name) as string;

    private void SetString(string name, string? value)
    {
        if (value == null)
            _key.DeleteValue(name);
        else
            _key.SetValue(name, value);
    }

    private int? GetInteger(string name) => _key.GetValue(name) as int?;

    private void SetInteger(string name, int? value)
    {
        if (value == null)
            _key.DeleteValue(name);
        else
            _key.SetValue(name, value);
    }
}
