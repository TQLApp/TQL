using Tql.Abstractions;

namespace Tql.Plugins.AzureDevOps.Services;

internal class ConfigurationManager
{
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;
    private readonly AzureDevOpsApi _api;

    private volatile Configuration _configuration = Configuration.Empty;

    public Configuration Configuration => _configuration;

    public event EventHandler? Changed;

    public ConfigurationManager(
        IConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager,
        AzureDevOpsApi api
    )
    {
        _peopleDirectoryManager = peopleDirectoryManager;
        _api = api;

        configurationManager.ConfigurationChanged += (_, e) =>
        {
            if (e.PluginId == AzureDevOpsPlugin.Id)
            {
                LoadConnections(e.Configuration);
                OnChanged();
            }
        };

        LoadConnections(configurationManager.GetConfiguration(AzureDevOpsPlugin.Id));
    }

    private void LoadConnections(string? json)
    {
        var configuration = default(Configuration);
        if (json != null)
            configuration = JsonSerializer.Deserialize<Configuration>(json);

        _configuration = configuration ?? Configuration.Empty;

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

        foreach (var connection in _configuration.Connections)
        {
            _peopleDirectoryManager.Add(new AzureDevOpsPeopleDirectory(connection, _api));
        }
    }

    protected virtual void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
