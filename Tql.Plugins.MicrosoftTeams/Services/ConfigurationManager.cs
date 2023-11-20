using Microsoft.Extensions.Logging;
using Tql.Abstractions;

namespace Tql.Plugins.MicrosoftTeams.Services;

internal class ConfigurationManager
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;
    private readonly ILogger<ConfigurationManager> _logger;
    private volatile Configuration _configuration = Configuration.Empty;
    private readonly object _syncRoot = new();
    private ImmutableArray<string> _directoryIds = ImmutableArray<string>.Empty;

    public Configuration Configuration => _configuration;

    public ImmutableArray<string> DirectoryIds
    {
        get
        {
            lock (_syncRoot)
            {
                return _directoryIds;
            }
        }
    }

    public ConfigurationManager(
        IConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager,
        ILogger<ConfigurationManager> logger
    )
    {
        _configurationManager = configurationManager;
        _peopleDirectoryManager = peopleDirectoryManager;
        _logger = logger;

        configurationManager.ConfigurationChanged += (_, e) =>
        {
            if (e.PluginId == MicrosoftTeamsPlugin.Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(MicrosoftTeamsPlugin.Id));
    }

    public bool HasDirectory(string id)
    {
        lock (_syncRoot)
        {
            return _directoryIds.Contains(id);
        }
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

        configuration ??= Configuration.Empty;

        _configuration = configuration;

        lock (_syncRoot)
        {
            if (configuration.Mode == ConfigurationMode.All)
            {
                _directoryIds = _peopleDirectoryManager
                    .Directories
                    .Select(p => p.Id)
                    .ToImmutableArray();
            }
            else
            {
                _directoryIds = configuration.DirectoryIds.ToImmutableArray();
            }
        }
    }

    public void UpdateConfiguration(Configuration configuration)
    {
        var json = JsonSerializer.Serialize(configuration);

        _configurationManager.SetConfiguration(MicrosoftTeamsPlugin.Id, json);
    }
}
