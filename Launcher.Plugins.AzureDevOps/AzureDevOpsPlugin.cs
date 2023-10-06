using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Categories;
using Launcher.Plugins.AzureDevOps.ConfigurationUI;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps;

[LauncherPlugin]
public class AzureDevOpsPlugin : ILauncherPlugin
{
    public static readonly Guid Id = Guid.Parse("36828080-c1f0-4759-8dff-2b764a44b62e");

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private MatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;
    private ImmutableArray<Connection> _connections;

    Guid ILauncherPlugin.Id => Id;

    public AzureDevOpsPlugin()
    {
        _matchTypeManagerBuilder = new MatchTypeManagerBuilder(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<RepositoriesMatch>();
        services.AddSingleton<ICacheManager<AzureData>, AzureCacheManager>();
        services.AddSingleton<IAzureDevOpsApi, AzureDevOpsApi>();
        services.AddSingleton<Images>();

        services.AddTransient<ConfigurationControl>();

        _matchTypeManagerBuilder.ConfigureServices(services);
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();

        configurationManager.RegisterConfigurationUIFactory(new ConfigurationUIFactory());

        configurationManager.ConfigurationChanged += (s, e) =>
        {
            if (e.PluginId == Id)
                LoadConnections(e.Configuration);
        };

        LoadConnections(configurationManager.GetConfiguration(Id));

        _matchTypeManager = _matchTypeManagerBuilder.Build(serviceProvider);

        void LoadConnections(string? json)
        {
            Configuration? configuration = null;
            if (json != null)
                configuration = JsonSerializer.Deserialize<Configuration>(json);

            _connections = configuration?.Connections ?? ImmutableArray<Connection>.Empty;
        }
    }

    public IEnumerable<IMatch> GetMatches()
    {
        var images = _serviceProvider!.GetRequiredService<Images>();
        var cache = _serviceProvider!.GetRequiredService<ICache<AzureData>>();
        var api = _serviceProvider!.GetRequiredService<IAzureDevOpsApi>();

        foreach (var connection in _connections)
        {
            yield return new BacklogsMatch(
                GetMatchName("Azure Backlog", connection),
                images,
                connection.Url,
                cache
            );

            yield return new BoardsMatch(
                GetMatchName("Azure Board", connection),
                images,
                connection.Url,
                cache
            );

            yield return new DashboardsMatch(
                GetMatchName("Azure Dashboard", connection),
                images,
                connection.Url,
                cache
            );

            yield return new RepositoriesMatch(
                GetMatchName("Azure Repository", connection),
                images,
                connection.Url,
                cache
            );

            yield return new NewsMatch(
                GetMatchName("Azure New", connection),
                images,
                connection.Url,
                cache
            );

            yield return new PipelinesMatch(
                GetMatchName("Azure Pipeline", connection),
                images,
                connection.Url,
                cache
            );

            yield return new QueriesMatch(
                GetMatchName("Azure Query", connection),
                images,
                connection.Url,
                cache,
                api
            );
        }

        string GetMatchName(string name, Connection connection)
        {
            if (_connections.Length > 1)
                return $"{name} ({connection.Name})";
            return name;
        }
    }

    public IMatch? DeserializeMatch(Guid typeId, string json)
    {
        return _matchTypeManager?.Deserialize(typeId, json);
    }

    private class ConfigurationUIFactory : IConfigurationUIFactory
    {
        public string Title => "Azure DevOps";

        public IConfigurationUI CreateControl(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ConfigurationControl>();
        }
    }
}
