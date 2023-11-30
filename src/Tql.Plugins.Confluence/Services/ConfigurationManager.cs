using Microsoft.Extensions.Logging;
using Tql.Abstractions;

namespace Tql.Plugins.Confluence.Services;

internal class ConfigurationManager
{
    private readonly IConfigurationManager _configurationManager;
    private readonly ConfluenceApi _api;
    private readonly ILogger<ConfigurationManager> _logger;
    private volatile Configuration _configuration = Configuration.Empty;

    public Configuration Configuration => _configuration;

    public event EventHandler? Changed;

    public ConfigurationManager(
        IConfigurationManager configurationManager,
        ConfluenceApi api,
        ILogger<ConfigurationManager> logger
    )
    {
        _configurationManager = configurationManager;
        _api = api;
        _logger = logger;

        configurationManager.ConfigurationChanged += (_, e) =>
        {
            if (e.PluginId == ConfluencePlugin.Id)
            {
                LoadConnections(e.Configuration);
                OnChanged();
            }
        };

        LoadConnections(configurationManager.GetConfiguration(ConfluencePlugin.Id));
    }

    public ConfluenceClient GetClient(string url)
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
    }

    public void UpdateConfiguration(Configuration configuration)
    {
        var json = JsonSerializer.Serialize(configuration);

        _configurationManager.SetConfiguration(ConfluencePlugin.Id, json);
    }

    protected virtual void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
