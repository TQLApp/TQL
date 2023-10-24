using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Confluence.Categories;
using Tql.Plugins.Confluence.ConfigurationUI;
using Tql.Plugins.Confluence.Data;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence;

[TqlPlugin]
public class ConfluencePlugin : ITqlPlugin
{
    public static readonly Guid Id = Guid.Parse("69a50b56-17f3-498b-bba6-3611a9c8cbb4");

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private MatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;

    public ConfluencePlugin()
    {
        _matchTypeManagerBuilder = new MatchTypeManagerBuilder(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ConfigurationManager>();
        services.AddSingleton<IconCacheManager>();
        services.AddSingleton<ConfluenceApi>();
        services.AddSingleton<ICacheManager<ConfluenceData>, ConfluenceCacheManager>();

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
        public string Title => "Confluence";

        public IConfigurationUI CreateControl(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ConfigurationControl>();
        }
    }
}