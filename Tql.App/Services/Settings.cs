﻿using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Win32;
using Tql.Abstractions;

namespace Tql.App.Services;

internal class Settings : INotifyPropertyChanged
{
    public const int DefaultHistoryInRootResults = 90;
    public const int DefaultCacheUpdateInterval = 60;
    public const int DefaultMainFontSize = 15;
    public const string DefaultMainWindowTint = "#40000000";

    // This is enabled by default because it does not track PII information.
    public const bool DefaultEnableMetricsTelemetry = true;

    // This is disabled by default because it may track PII information. It's
    // very difficult to control PII information in exceptions. They can very
    // easily include e.g. user names in paths.
    public const bool DefaultEnableExceptionTelemetry = false;

    public const int DefaultTextOuterGlowSize = 3;

    private readonly RegistryKey _key;

    public string? HotKey
    {
        get => GetString(nameof(HotKey));
        set => SetString(nameof(HotKey), value);
    }

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

    public string? MainWindowTint
    {
        get => GetString(nameof(MainWindowTint));
        set => SetString(nameof(MainWindowTint), value);
    }

    public string? Theme
    {
        get => GetString(nameof(Theme));
        set => SetString(nameof(Theme), value);
    }

    public string? DeviceId
    {
        get => GetString(nameof(DeviceId));
        set => SetString(nameof(DeviceId), value);
    }

    public string? UserId
    {
        get => GetString(nameof(UserId));
        set => SetString(nameof(UserId), value);
    }

    public bool? EnableMetricsTelemetry
    {
        get => GetBoolean(nameof(EnableMetricsTelemetry));
        set => SetBoolean(nameof(EnableMetricsTelemetry), value);
    }

    public bool? EnableExceptionTelemetry
    {
        get => GetBoolean(nameof(EnableExceptionTelemetry));
        set => SetBoolean(nameof(EnableExceptionTelemetry), value);
    }

    public int? TextOuterGlowSize
    {
        get => GetInteger(nameof(TextOuterGlowSize));
        set => SetInteger(nameof(TextOuterGlowSize), value);
    }

    public string? Language
    {
        get => GetString(nameof(Language));
        set => SetString(nameof(Language), value);
    }

    public string? QuickStart
    {
        get => GetString(nameof(QuickStart));
        set => SetString(nameof(QuickStart), value);
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

    public Settings(IStore store)
    {
        _key = ((Store)store).CreateBaseKey();
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
