using Tql.Abstractions;

namespace Tql.Plugins.MicrosoftTeams.Services;

internal class ConfigurationManager
{
    private readonly IPeopleDirectoryManager _peopleDirectoryManager;
    private volatile Configuration _configuration = Configuration.Empty;
    private volatile HashSet<string> _directoryIds = new();

    public Configuration Configuration => _configuration;
    public ImmutableArray<string> DirectoryIds => _directoryIds.ToImmutableArray();

    public ConfigurationManager(
        IConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager
    )
    {
        _peopleDirectoryManager = peopleDirectoryManager;
        configurationManager.ConfigurationChanged += (s, e) =>
        {
            if (e.PluginId == MicrosoftTeamsPlugin.Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(MicrosoftTeamsPlugin.Id));
    }

    public bool HasDirectory(string id) => _directoryIds.Contains(id);

    private void LoadConnections(string? json)
    {
        var configuration = default(Configuration);
        if (json != null)
            configuration = JsonSerializer.Deserialize<Configuration>(json);

        _configuration = configuration ?? Configuration.Empty;

        if (_configuration.Mode == ConfigurationMode.All)
        {
            _directoryIds = new HashSet<string>(
                _peopleDirectoryManager.Directories.Select(p => p.Id)
            );
        }
        else
        {
            _directoryIds = new HashSet<string>(_configuration.DirectoryIds);
        }
    }
}
