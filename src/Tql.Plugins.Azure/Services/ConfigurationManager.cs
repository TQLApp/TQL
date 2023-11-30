using Microsoft.Extensions.Logging;
using Tql.Abstractions;

namespace Tql.Plugins.Azure.Services;

internal class ConfigurationManager
{
    private readonly IConfigurationManager _configurationManager;
    private readonly ILogger _logger;
    private volatile Configuration _configuration = Configuration.Empty;

    public Configuration Configuration => _configuration;

    public ConfigurationManager(
        IConfigurationManager configurationManager,
        ILogger<ConfigurationManager> logger
    )
    {
        _configurationManager = configurationManager;
        _logger = logger;

        configurationManager.ConfigurationChanged += (_, e) =>
        {
            if (e.PluginId == AzurePlugin.Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(AzurePlugin.Id));
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

        _configurationManager.SetConfiguration(AzurePlugin.Id, json);
    }
}
