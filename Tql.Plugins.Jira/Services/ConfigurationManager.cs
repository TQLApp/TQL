using Tql.Abstractions;

namespace Tql.Plugins.Jira.Services;

internal class ConfigurationManager
{
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;
    private readonly JiraApi _api;
    private volatile Configuration _configuration = Configuration.Empty;

    public Configuration Configuration => _configuration;

    public ConfigurationManager(
        IConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager,
        JiraApi api
    )
    {
        _peopleDirectoryManager = peopleDirectoryManager;
        _api = api;

        configurationManager.ConfigurationChanged += (s, e) =>
        {
            if (e.PluginId == JiraPlugin.Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(JiraPlugin.Id));
    }

    public JiraClient GetClient(string url)
    {
        return _api.GetClient(_configuration.Connections.Single(p => p.Url == url));
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
        foreach (var directory in _peopleDirectoryManager.Directories.OfType<JiraPeopleDirectory>())
        {
            _peopleDirectoryManager.Remove(directory);
        }

        foreach (var connection in _configuration.Connections)
        {
            _peopleDirectoryManager.Add(new JiraPeopleDirectory(connection, _api));
        }
    }
}
