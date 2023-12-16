using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.Services;

internal class LocalSettings : INotifyPropertyChanged
{
    private readonly RegistryKey _key;

    // We need the language very early in the startup process. Because of this,
    // we have a language setting in both Settings and LocalSettings that need
    // to be synced.
    public string? Language
    {
        get => GetString(nameof(Language));
        set => SetString(nameof(Language), value);
    }

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

    public string? SynchronizationConfiguration
    {
        get => GetString(nameof(SynchronizationConfiguration));
        set => SetString(nameof(SynchronizationConfiguration), value);
    }

    public string? LastSynchronization
    {
        get => GetString(nameof(LastSynchronization));
        set => SetString(nameof(LastSynchronization), value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public LocalSettings(IStore store, Settings settings)
    {
        _key = ((Store)store).CreateBaseKey();

        this.AttachPropertyChanged(
            nameof(Language),
            (_, _) =>
            {
                if (Language != settings.Language)
                    settings.Language = Language;
            }
        );

        settings.AttachPropertyChanged(
            nameof(Language),
            (_, _) =>
            {
                if (Language != settings.Language)
                    Language = settings.Language;
            }
        );
    }

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
