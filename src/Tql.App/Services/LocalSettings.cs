using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using Tql.Abstractions;

namespace Tql.App.Services;

internal class LocalSettings(IStore store) : INotifyPropertyChanged
{
    private readonly RegistryKey _key = ((Store)store).CreateBaseKey();

    public string? DeviceId
    {
        get => GetString(nameof(DeviceId));
        set => SetString(nameof(DeviceId), value);
    }

    public string? EncryptionKey
    {
        get => GetString(nameof(EncryptionKey));
        set => SetString(nameof(EncryptionKey), value);
    }

    public bool? InstallPrerelease
    {
        get => GetBoolean(nameof(InstallPrerelease));
        set => SetBoolean(nameof(InstallPrerelease), value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private string? GetString(string name) => _key.GetValue(name) as string;

    private void SetString(string name, string? value) => SetValue(name, value);

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

    private void SetInteger(string name, int? value) => SetValue(name, value);

    private void SetValue(string name, object? value)
    {
        if (value == null)
            _key.DeleteValue(name, false);
        else
            _key.SetValue(name, value);

        RaisePropertyChanged(name);
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
