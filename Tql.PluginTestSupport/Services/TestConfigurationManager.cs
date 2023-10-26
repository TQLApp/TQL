using System.Collections.Concurrent;
using Tql.Abstractions;

namespace Tql.PluginTestSupport.Services;

internal class TestConfigurationManager : IConfigurationManager
{
    private readonly ConcurrentDictionary<Guid, string?> _configurations = new();

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public string? GetConfiguration(Guid pluginId)
    {
        _configurations.TryGetValue(pluginId, out var configuration);
        return configuration;
    }

    public void SetConfiguration(Guid pluginId, string? configuration)
    {
        _configurations[pluginId] = configuration;

        OnConfigurationChanged(new ConfigurationChangedEventArgs(pluginId, configuration));
    }

    protected virtual void OnConfigurationChanged(ConfigurationChangedEventArgs e) =>
        ConfigurationChanged?.Invoke(this, e);
}
