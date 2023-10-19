using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Tql.Abstractions;

namespace Tql.App.Services;

internal class Settings : INotifyPropertyChanged
{
    public const int DefaultHistoryInRootResults = 90;
    public const int DefaultCacheUpdateInterval = 30;
    public const int DefaultMainFontSize = 15;

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

    public int? CacheUpdateInterval
    {
        get => GetInteger(nameof(CacheUpdateInterval));
        set => SetInteger(nameof(CacheUpdateInterval), value);
    }

    public int? MainFontSize
    {
        get => GetInteger(nameof(MainFontSize));
        set => SetInteger(nameof(MainFontSize), value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

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

        RaisePropertyChanged(name);
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

        RaisePropertyChanged(name);
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
