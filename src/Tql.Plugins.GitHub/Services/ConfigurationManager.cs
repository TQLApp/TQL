using Microsoft.Extensions.Logging;
using Tql.Abstractions;

namespace Tql.Plugins.GitHub.Services;

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
            if (e.PluginId == GitHubPlugin.Id)
            {
                LoadConnections(e.Configuration);
                OnChanged();
            }
        };

        LoadConnections(configurationManager.GetConfiguration(GitHubPlugin.Id));
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

    public void UpdateCredentials(Guid id, string? protectedCredentials)
    {
        var connection = _configuration.Connections.Single(p => p.Id == id);

        UpdateConfiguration(
            new Configuration(
                _configuration
                    .Connections
                    .Replace(
                        connection,
                        connection with
                        {
                            ProtectedCredentials = protectedCredentials
                        }
                    )
            )
        );
    }

    public void UpdateConfiguration(Configuration configuration)
    {
        var json = JsonSerializer.Serialize(configuration);

        _configurationManager.SetConfiguration(GitHubPlugin.Id, json);
    }

    protected virtual void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
