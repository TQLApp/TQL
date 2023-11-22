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
    public static readonly Guid ConfigurationPageId = Guid.Parse(
        "12e42adb-7f02-40fe-b8ba-2938b49b3d81"
    );

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private IMatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;
    public string Title => Labels.AzureDevOpsPlugin_Title;

    public AzureDevOpsPlugin()
    {
        _matchTypeManagerBuilder = MatchTypeManagerBuilder.ForAssembly(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ICacheManager<AzureData>, AzureCacheManager>();
        services.AddSingleton<AzureDevOpsApi>();
        services.AddSingleton<ConfigurationManager>();
        services.AddSingleton<AzureWorkItemIconManager>();
        services.AddSingleton<AzureDevOpsPeopleDirectoryManager>();

        services.AddTransient<ConfigurationControl>();

        _matchTypeManagerBuilder.ConfigureServices(services);
    }

    public void Initialize(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;

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

    public IMatch? DeserializeMatch(Guid typeId, string value)
    {
        return _matchTypeManager?.Deserialize(typeId, value);
    }

    public IEnumerable<IConfigurationPage> GetConfigurationPages()
    {
        yield return _serviceProvider!.GetRequiredService<ConfigurationControl>();
    }
}
