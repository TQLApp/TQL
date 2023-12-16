using System.IO;
using System.Text.Json.Nodes;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services;

internal class ConfigurationManager : IConfigurationManager
{
    private readonly IStore _store;
    private readonly object _syncRoot = new();
    private readonly Dictionary<Guid, string> _configuration = new();

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationManager(IStore store)
    {
        _store = store;

        LoadConfiguration();
    }

    public string? GetConfiguration(Guid pluginId)
    {
        lock (_syncRoot)
        {
            _configuration.TryGetValue(pluginId, out var configuration);
            return configuration;
        }
    }

    public void SetConfiguration(Guid pluginId, string? configuration)
    {
        lock (_syncRoot)
        {
            var oldConfiguration = GetConfiguration(pluginId);
            if (configuration == oldConfiguration)
                return;

            if (configuration == null)
            {
                _configuration.Remove(pluginId);
            }
            else
            {
                try
                {
                    JsonNode.Parse(configuration);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Configuration must be valid JSON", ex);
                }

                _configuration[pluginId] = configuration;
            }

            SaveConfiguration();
        }

        OnConfigurationChanged(new ConfigurationChangedEventArgs(pluginId, configuration));
    }

    private void LoadConfiguration()
    {
        var fileName = GetConfigurationFileName();

        if (File.Exists(fileName))
        {
            var obj = (JsonObject)JsonNode.Parse(File.ReadAllText(fileName))!;

            foreach (var entry in obj)
            {
                _configuration[Guid.Parse(entry.Key)] = entry.Value!.ToJsonString();
            }
        }
    }

    private void SaveConfiguration()
    {
        using var stream = File.Create(GetConfigurationFileName());

        SaveConfiguration(stream);
    }

    public void SaveConfiguration(Stream stream)
    {
        var obj = new JsonObject();

        foreach (var entry in _configuration)
        {
            obj.Add(entry.Key.ToString(), JsonNode.Parse(entry.Value));
        }

        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true });

        obj.WriteTo(writer);
    }

    public void RestoreConfiguration(Stream stream)
    {
        using var target = File.Create(GetConfigurationFileName());

        stream.CopyTo(target);
    }

    private string GetConfigurationFileName() =>
        Path.Combine(((Store)_store).DataFolder, "Configuration.json");

    protected virtual void OnConfigurationChanged(ConfigurationChangedEventArgs e) =>
        ConfigurationChanged?.Invoke(this, e);
}
