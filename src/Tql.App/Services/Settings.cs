using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using Tql.Abstractions;

namespace Tql.App.Services;

internal partial class Settings : INotifyPropertyChanged
{
    private readonly IConfigurationManager _configurationManager;

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

    private readonly ConcurrentDictionary<string, object> _values = new();

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

    // We need the language very early in the startup process. Because of this,
    // we have a language setting in both Settings and LocalSettings that need
    // to be synced.
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

    public event PropertyChangedEventHandler? PropertyChanged;

    public Settings(IStore store, IConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;

        MigrateRegistrySettings(store, configurationManager);

        var configuration = configurationManager.GetConfiguration(
            Constants.SettingsConfigurationId
        );

        if (configuration != null)
        {
            var obj = JsonNode.Parse(configuration)!.AsObject();

            foreach (var entry in obj)
            {
                switch (entry.Value?.GetValueKind())
                {
                    case JsonValueKind.String:
                        _values[entry.Key] = entry.Value.GetValue<string>();
                        break;
                    case JsonValueKind.Number:
                        _values[entry.Key] = entry.Value.GetValue<int>();
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        _values[entry.Key] = entry.Value.GetValue<bool>();
                        break;
                }
            }
        }
    }

    private string? GetString(string name) => GetValue(name) as string;

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

    private int? GetInteger(string name) => GetValue(name) as int?;

    private void SetInteger(string name, int? value) => SetValue(name, value);

    private void SetValue(string name, object? value)
    {
        var oldValue = GetValue(name);

        if (Equals(value, oldValue))
            return;

        if (value == null)
            _values.TryRemove(name, out _);
        else
            _values[name] = value;

        WriteConfiguration();

        RaisePropertyChanged(name);
    }

    private object? GetValue(string name)
    {
        _values.TryGetValue(name, out var value);
        return value;
    }

    private void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void WriteConfiguration()
    {
        var obj = new JsonObject();

        foreach (var entry in _values)
        {
            var jsonValue = entry.Value switch
            {
                int intValue => JsonValue.Create(intValue),
                string stringValue => JsonValue.Create(stringValue),
                bool boolValue => JsonValue.Create(boolValue),
                _ => null
            };

            if (jsonValue != null)
                obj.Add(entry.Key, jsonValue);
        }

        _configurationManager.SetConfiguration(
            Constants.SettingsConfigurationId,
            obj.ToJsonString()
        );
    }
}
