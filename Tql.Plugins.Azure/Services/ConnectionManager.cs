using Tql.Abstractions;

namespace Tql.Plugins.Azure.Services;

internal class ConnectionManager
{
    public ImmutableArray<Connection> Connections { get; private set; }

    public ConnectionManager(IConfigurationManager configurationManager)
    {
        configurationManager.ConfigurationChanged += (s, e) =>
        {
            if (e.PluginId == AzurePlugin.Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(AzurePlugin.Id));

        void LoadConnections(string? json)
        {
            var configuration = default(Configuration);
            if (json != null)
                configuration = JsonSerializer.Deserialize<Configuration>(json);

            Connections = configuration?.Connections ?? ImmutableArray<Connection>.Empty;
        }
    }
}
