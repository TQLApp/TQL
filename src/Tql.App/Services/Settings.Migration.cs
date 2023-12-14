using System.Text.Json.Nodes;
using Tql.Abstractions;

namespace Tql.App.Services;

internal partial class Settings
{
    private void MigrateRegistrySettings(IStore store, IConfigurationManager configurationManager)
    {
        var configuration = configurationManager.GetConfiguration(
            Constants.SettingsConfigurationId
        );
        if (configuration != null)
            return;

        using var key = ((Store)store).CreateBaseKey();

        var values = new Dictionary<string, JsonValue?>
        {
            {
                nameof(EnableExceptionTelemetry),
                GetLegacyBoolean(nameof(EnableExceptionTelemetry))
            },
            { nameof(EnableMetricsTelemetry), GetLegacyBoolean(nameof(EnableMetricsTelemetry)) },
            { nameof(CacheUpdateInterval), GetLegacyInteger(nameof(CacheUpdateInterval)) },
            { nameof(HistoryInRootResults), GetLegacyInteger(nameof(HistoryInRootResults)) },
            { nameof(MainFontSize), GetLegacyInteger(nameof(MainFontSize)) },
            { nameof(TextOuterGlowSize), GetLegacyInteger(nameof(TextOuterGlowSize)) },
            { nameof(HotKey), GetLegacyString(nameof(HotKey)) },
            { nameof(Language), GetLegacyString(nameof(Language)) },
            { nameof(MainWindowTint), GetLegacyString(nameof(MainWindowTint)) },
            { nameof(QuickStart), GetLegacyString(nameof(QuickStart)) },
            { nameof(ShowOnScreen), GetLegacyString(nameof(ShowOnScreen)) },
            { nameof(Theme), GetLegacyString(nameof(Theme)) },
            { nameof(UserId), GetLegacyString(nameof(UserId)) },
        };

        var obj = new JsonObject();

        foreach (var entry in values.Where(p => p.Value != null))
        {
            obj.Add(entry.Key, entry.Value);
        }

        configurationManager.SetConfiguration(
            Constants.SettingsConfigurationId,
            obj.ToJsonString()
        );

        foreach (var name in values.Keys)
        {
            // We need the language very early in the startup process. Because of this,
            // we have a language setting in both Settings and LocalSettings that need
            // to be synced.
            if (name != nameof(LocalSettings.Language))
                key.DeleteValue(name, false);
        }

        JsonValue? GetLegacyString(string name) =>
            key.GetValue(name) switch
            {
                string stringValue => JsonValue.Create(stringValue),
                _ => null
            };

        JsonValue? GetLegacyBoolean(string name) =>
            key.GetValue(name) switch
            {
                0 => JsonValue.Create(false),
                int => JsonValue.Create(true),
                _ => null
            };

        JsonValue? GetLegacyInteger(string name) =>
            key.GetValue(name) switch
            {
                int intValue => JsonValue.Create(intValue),
                _ => null
            };
    }
}
