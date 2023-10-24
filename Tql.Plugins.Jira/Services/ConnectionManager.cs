using Tql.Abstractions;

namespace Tql.Plugins.Jira.Services;

internal class ConnectionManager
{
    public ImmutableArray<Connection> Connections { get; private set; }

    public ConnectionManager(IConfigurationManager configurationManager)
    {
        configurationManager.ConfigurationChanged += (s, e) =>
        {
            if (e.PluginId == JiraPlugin.Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(JiraPlugin.Id));

        void LoadConnections(string? json)
        {
            var configuration = default(Configuration);
            if (json != null)
                configuration = JsonSerializer.Deserialize<Configuration>(json);

            Connections = configuration?.Connections ?? ImmutableArray<Connection>.Empty;
        }
    }
}
