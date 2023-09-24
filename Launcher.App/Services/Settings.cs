using Microsoft.Win32;

namespace Launcher.App.Services;

internal class Settings
{
    private readonly RegistryKey _key;

    public string? ShowOnScreen
    {
        get => GetString(nameof(ShowOnScreen));
        set => SetString(nameof(ShowOnScreen), value);
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
}
