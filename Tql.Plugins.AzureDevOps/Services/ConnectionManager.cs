using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Services;

internal class ConnectionManager
{
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;
    private readonly AzureDevOpsApi _api;

    public ImmutableArray<Connection> Connections { get; private set; }

    public ConnectionManager(
        IConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager,
        AzureDevOpsApi api
    )
    {
        _peopleDirectoryManager = peopleDirectoryManager;
        _api = api;

        configurationManager.ConfigurationChanged += (s, e) =>
        {
            if (e.PluginId == AzureDevOpsPlugin.Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(AzureDevOpsPlugin.Id));
    }

    private void LoadConnections(string? json)
    {
        var configuration = default(Configuration);
        if (json != null)
            configuration = JsonSerializer.Deserialize<Configuration>(json);

        Connections = configuration?.Connections ?? ImmutableArray<Connection>.Empty;

        UpdatePeopleDirectoryManager();
    }

    private void UpdatePeopleDirectoryManager()
    {
        foreach (
            var directory in _peopleDirectoryManager.Directories.OfType<AzureDevOpsPeopleDirectory>()
        )
        {
            _peopleDirectoryManager.Remove(directory);
        }

        foreach (var connection in Connections)
        {
            _peopleDirectoryManager.Add(new AzureDevOpsPeopleDirectory(connection, _api));
        }
    }
}
