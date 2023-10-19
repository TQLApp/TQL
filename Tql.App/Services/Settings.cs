using Microsoft.Win32;
using Tql.Abstractions;

namespace Tql.App.Services;

internal class Settings
{
    public const int DefaultHistoryInRootResults = 90;

    private readonly RegistryKey _key;

    public string? ShowOnScreen
    {
        get => GetString(nameof(ShowOnScreen));
        set => SetString(nameof(ShowOnScreen), value);
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

    private bool? GetBoolean(string name) =>
        GetInteger(name) switch
        {
            null => null,
            0 => false,
            _ => true
        };

    private void SetBoolean(string name, bool? value) =>
        SetInteger(
            name,
            value switch
            {
                null => null,
                true => 1,
                false => 0
            }
        );

    private int? GetInteger(string name) => _key.GetValue(name) as int?;

    private void SetInteger(string name, int? value)
    {
        if (value == null)
            _key.DeleteValue(name);
        else
            _key.SetValue(name, value);
    }
}
