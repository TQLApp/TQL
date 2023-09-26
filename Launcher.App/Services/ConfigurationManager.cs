using Launcher.Abstractions;
using System.IO;
using System.Text.Json.Nodes;

namespace Launcher.App.Services;

internal class ConfigurationManager : IConfigurationManager
{
    private readonly object _syncRoot = new();
    private readonly Dictionary<Guid, string> _configuration = new();
    private readonly List<IConfigurationUIFactory> _configurationUIFactories = new();

    public ImmutableArray<IConfigurationUIFactory> ConfigurationUIFactories
    {
        get
        {
            lock (_syncRoot)
            {
                return _configurationUIFactories.ToImmutableArray();
            }
        }
    }

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationManager(IStore store)
    {
        var fileName = Path.Combine(store.UserSettingsFolder, "Configuration.json");

        if (File.Exists(fileName))
        {
            var obj = (JsonObject)JsonNode.Parse(File.ReadAllText(fileName))!;

            foreach (var entry in obj)
            {
                _configuration[Guid.Parse(entry.Key)] = entry.Value!.ToJsonString();
            }
        }
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
        }

        OnConfigurationChanged(new ConfigurationChangedEventArgs(pluginId, configuration));
    }

    public void RegisterConfigurationUIFactory(IConfigurationUIFactory factory)
    {
        lock (_syncRoot)
        {
            _configurationUIFactories.Add(factory);
        }
    }

    protected virtual void OnConfigurationChanged(ConfigurationChangedEventArgs e)
    {
        ConfigurationChanged?.Invoke(this, e);
    }
}
