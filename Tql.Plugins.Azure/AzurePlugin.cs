using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Azure.Categories;
using Tql.Plugins.Azure.ConfigurationUI;
using Tql.Plugins.Azure.Services;
using Tql.Plugins.Azure.Support;
using Tql.Utilities;

namespace Tql.Plugins.Azure;

[TqlPlugin]
public class AzurePlugin : ITqlPlugin
{
    public static readonly Guid Id = Guid.Parse("51ebb93a-4d72-4231-adf0-4985f377a1b7");
    public static readonly Guid ConfigurationPageId = Guid.Parse(
        "83bee2a3-4797-4ef9-a32a-ad9a3108a0a8"
    );

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private IMatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;
    public string Title => Labels.AzurePlugin_Title;

    public AzurePlugin()
    {
        _matchTypeManagerBuilder = MatchTypeManagerBuilder.ForAssembly(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<AzureApi>();
        services.AddSingleton<ConfigurationManager>();

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
            let json = JsonSerializer.Serialize(new RootItemDto(connection.Id))
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
