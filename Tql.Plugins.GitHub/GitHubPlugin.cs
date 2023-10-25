using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Categories;
using Tql.Plugins.GitHub.ConfigurationUI;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub;

[TqlPlugin]
public class GitHubPlugin : ITqlPlugin
{
    public static readonly Guid Id = Guid.Parse("028ffb5f-5d9f-4ee1-91fd-47f192d16e20");
    public static readonly Guid ConfigurationUIId = Guid.Parse(
        "35954273-99ae-473c-9386-2dc220a12c45"
    );

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private MatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;

    public GitHubPlugin()
    {
        _matchTypeManagerBuilder = new MatchTypeManagerBuilder(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<GitHubApi>();
        services.AddSingleton<ConfigurationManager>();
        services.AddSingleton<ICacheManager<GitHubData>, GitHubCacheManager>();

        services.AddTransient<ConfigurationControl>();

        _matchTypeManagerBuilder.ConfigureServices(services);
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

        var configurationManager = serviceProvider.GetRequiredService<IConfigurationManager>();

        configurationManager.RegisterConfigurationUIFactory(new ConfigurationUIFactory());

        _matchTypeManager = _matchTypeManagerBuilder.Build(serviceProvider);
    }

    public IEnumerable<IMatch> GetMatches()
    {
        var connectionManager = _serviceProvider!.GetRequiredService<ConfigurationManager>();

        foreach (var connection in connectionManager.Configuration.Connections)
        {
            foreach (var matchType in _matchTypeManager!.MatchTypes)
            {
                var attribute = matchType.GetType().GetCustomAttribute<RootMatchTypeAttribute>();
                if (attribute == null)
                    continue;

                var json = JsonSerializer.Serialize(
                    new RootItemDto(connection.Id, RootItemScope.Global)
                );

                yield return matchType.Deserialize(json)!;

                if (attribute.SupportsUserScope)
                {
                    json = JsonSerializer.Serialize(
                        new RootItemDto(connection.Id, RootItemScope.User)
                    );

                    yield return matchType.Deserialize(json)!;
                }
            }
        }
    }

    public IMatch? DeserializeMatch(Guid typeId, string json)
    {
        return _matchTypeManager?.Deserialize(typeId, json);
    }

    private class ConfigurationUIFactory : IConfigurationUIFactory
    {
        public Guid Id => ConfigurationUIId;
        public string Title => "GitHub";

        public IConfigurationUI CreateControl(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ConfigurationControl>();
        }
    }
}
