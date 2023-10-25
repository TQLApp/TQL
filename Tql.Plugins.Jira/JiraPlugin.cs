using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Categories;
using Tql.Plugins.Jira.ConfigurationUI;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira;

[TqlPlugin]
public class JiraPlugin : ITqlPlugin
{
    public static readonly Guid Id = Guid.Parse("18760188-f7b1-448d-94ba-646b85b55d98");
    public static readonly Guid ConfigurationUIId = Guid.Parse(
        "97f80138-0b44-4ffc-b5c2-2a40f1070e17"
    );

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private MatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;

    public JiraPlugin()
    {
        _matchTypeManagerBuilder = new MatchTypeManagerBuilder(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ConfigurationManager>();
        services.AddSingleton<IconCacheManager>();
        services.AddSingleton<JiraApi>();
        services.AddSingleton<ICacheManager<JiraData>, JiraCacheManager>();

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

        return from connection in connectionManager.Configuration.Connections
            let json = JsonSerializer.Serialize(new RootItemDto(connection.Url))
            from matchType in _matchTypeManager!.MatchTypes
            where matchType.GetType().GetCustomAttribute<RootMatchTypeAttribute>() != null
            select matchType.Deserialize(json);
    }

    public IMatch? DeserializeMatch(Guid typeId, string json)
    {
        return _matchTypeManager?.Deserialize(typeId, json);
    }

    private class ConfigurationUIFactory : IConfigurationUIFactory
    {
        public Guid Id => ConfigurationUIId;
        public string Title => "JIRA";

        public IConfigurationUI CreateControl(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ConfigurationControl>();
        }
    }
}
