using Tql.Abstractions;

namespace Tql.Plugins.MicrosoftTeams.Services;

internal class ConfigurationManager
{
    private volatile Configuration _configuration = Configuration.Empty;

    public Configuration Configuration => _configuration;

    public ConfigurationManager(IConfigurationManager configurationManager)
    {
        configurationManager.ConfigurationChanged += (s, e) =>
        {
            if (e.PluginId == MicrosoftTeamsPlugin.Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(MicrosoftTeamsPlugin.Id));
    }

    private void LoadConnections(string? json)
    {
        var configuration = default(Configuration);
        if (json != null)
            configuration = JsonSerializer.Deserialize<Configuration>(json);

        _configuration = configuration ?? Configuration.Empty;
    }
}
