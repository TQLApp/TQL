using Microsoft.Extensions.Logging;
using Tql.Abstractions;

namespace Tql.Plugins.Jira.Services;

internal class ConfigurationManager
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;
    private readonly JiraApi _api;
    private readonly ILogger<ConfigurationManager> _logger;
    private volatile Configuration _configuration = Configuration.Empty;

    public Configuration Configuration => _configuration;

    public event EventHandler? Changed;

    public ConfigurationManager(
        IConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager,
        JiraApi api,
        ILogger<ConfigurationManager> logger
    )
    {
        _configurationManager = configurationManager;
        _peopleDirectoryManager = peopleDirectoryManager;
        _api = api;
        _logger = logger;

        configurationManager.ConfigurationChanged += (_, e) =>
        {
            if (e.PluginId == JiraPlugin.Id)
            {
                LoadConnections(e.Configuration);
                OnChanged();
            }
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
        {
            try
            {
                configuration = JsonSerializer.Deserialize<Configuration>(json);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to deserialize configuration");
            }
        }

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

    public void UpdateConfiguration(Configuration configuration)
    {
        var json = JsonSerializer.Serialize(configuration);

        _configurationManager.SetConfiguration(JiraPlugin.Id, json);
    }

    protected virtual void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
