using Microsoft.Extensions.Logging;
using Tql.Abstractions;

namespace Tql.Plugins.Outlook.Services;

internal class ConfigurationManager
{
    private readonly IConfigurationManager _configurationManager;
    private readonly ILogger<ConfigurationManager> _logger;
    private volatile Configuration _configuration = Configuration.Empty;

    public Configuration Configuration => _configuration;

    public event EventHandler? Changed;

    public ConfigurationManager(
        IConfigurationManager configurationManager,
        ILogger<ConfigurationManager> logger
    )
    {
        _configurationManager = configurationManager;
        _logger = logger;

        configurationManager.ConfigurationChanged += (_, e) =>
        {
            if (e.PluginId == OutlookPlugin.Id)
                LoadConfiguration(e.Configuration);
        };

        LoadConfiguration();
    }

    private void LoadConfiguration()
    {
        LoadConfiguration(_configurationManager.GetConfiguration(OutlookPlugin.Id));
    }

    private void LoadConfiguration(string? json)
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

        configuration ??= Configuration.Empty;

        _configuration = configuration;

        OnChanged();
    }

    public void UpdateConfiguration(Configuration configuration)
    {
        var json = JsonSerializer.Serialize(configuration);

        _configurationManager.SetConfiguration(OutlookPlugin.Id, json);
    }

    protected virtual void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
