using Tql.Abstractions;

namespace Tql.Plugins.Confluence.Services;

internal class ConfigurationManager
{
    private readonly ConfluenceApi _api;
    private volatile Configuration _configuration = Configuration.Empty;

    public Configuration Configuration => _configuration;

    public event EventHandler? Changed;

    public ConfigurationManager(IConfigurationManager configurationManager, ConfluenceApi api)
    {
        _api = api;

        configurationManager.ConfigurationChanged += (s, e) =>
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
            configuration = JsonSerializer.Deserialize<Configuration>(json);

        _configuration = configuration ?? Configuration.Empty;
    }

    protected virtual void OnChanged() => Changed?.Invoke(this, EventArgs.Empty);
}
