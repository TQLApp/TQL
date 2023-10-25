using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Categories;
using Tql.Plugins.AzureDevOps.ConfigurationUI;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps;

[TqlPlugin]
public class AzureDevOpsPlugin : ITqlPlugin
{
    public static readonly Guid Id = Guid.Parse("36828080-c1f0-4759-8dff-2b764a44b62e");
    public static readonly Guid ConfigurationUIId = Guid.Parse(
        "12e42adb-7f02-40fe-b8ba-2938b49b3d81"
    );

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private MatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;

    public AzureDevOpsPlugin()
    {
        _matchTypeManagerBuilder = new MatchTypeManagerBuilder(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICacheManager<AzureData>, AzureCacheManager>();
        services.AddSingleton<AzureDevOpsApi>();
        services.AddSingleton<ConfigurationManager>();
        services.AddSingleton<AzureWorkItemIconManager>();

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
        public string Title => "Azure DevOps";

        public IConfigurationUI CreateControl(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<ConfigurationControl>();
        }
    }
}
