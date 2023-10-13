using Launcher.Abstractions;

namespace Launcher.Plugins.GitHub.Services;

internal class ConnectionManager
{
    public ImmutableArray<Connection> Connections { get; private set; }

    public ConnectionManager(IConfigurationManager configurationManager)
    {
        configurationManager.ConfigurationChanged += (s, e) =>
        {
            if (e.PluginId == GitHubPlugin.Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(GitHubPlugin.Id));

        void LoadConnections(string? json)
        {
            var configuration = default(Configuration);
            if (json != null)
                configuration = JsonSerializer.Deserialize<Configuration>(json);

            Connections = configuration?.Connections ?? ImmutableArray<Connection>.Empty;
        }
    }
}
