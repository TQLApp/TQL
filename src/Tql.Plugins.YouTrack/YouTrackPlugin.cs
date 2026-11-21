using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.YouTrack.Categories;
using Tql.Plugins.YouTrack.ConfigurationUI;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack;

[TqlPlugin]
public class YouTrackPlugin : ITqlPlugin
{
    public static readonly Guid Id = Guid.Parse("86f43436-8df5-49be-8c6d-568b3a91e47c");
    public static readonly Guid ConfigurationPageId = Guid.Parse(
        "ea0b3734-5457-413d-8b00-17490743fa37"
    );

    private readonly MatchTypeManagerBuilder _matchTypeManagerBuilder;
    private IMatchTypeManager? _matchTypeManager;
    private IServiceProvider? _serviceProvider;

    Guid ITqlPlugin.Id => Id;
    public string Title => Labels.YouTrackPlugin_Title;

    public YouTrackPlugin()
    {
        _matchTypeManagerBuilder = MatchTypeManagerBuilder.ForAssembly(GetType().Assembly);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<ConfigurationManager>();
        services.AddSingleton<IconCacheManager>();
        services.AddSingleton<YouTrackApi>();
        services.AddSingleton<ICacheManager<YouTrackData>, YouTrackCacheManager>();

        services.AddTransient<ConfigurationControl>();
        services.AddTransient<ConnectionEditWindow>();

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
        var data = GetLoadedData();

        return from connection in connectionManager.Configuration.Connections
            let json = JsonSerializer.Serialize(new RootItemDto(connection.Url))
            from matchType in _matchTypeManager!.MatchTypes
            let attribute = matchType.GetType().GetCustomAttribute<RootMatchTypeAttribute>()
            where attribute != null && IsAvailable(data, connection.Url, attribute.Endpoint)
            select matchType.Deserialize(json);
    }

    // The cached data if it's loaded, without waiting for it.
    private YouTrackData? GetLoadedData()
    {
        var cache = _serviceProvider!.GetRequiredService<ICache<YouTrackData>>();
        if (!cache.IsAvailable)
            return null;

        var task = cache.Get();

        return task.IsCompletedSuccessfully ? task.Result : null;
    }

    // A category whose endpoint answered 403 or 404 when the cache was
    // loaded is left out for that connection. Until the cache is loaded,
    // every category is offered.
    private static bool IsAvailable(YouTrackData? data, string url, YouTrackEndpoint? endpoint)
    {
        if (data == null || endpoint == null)
            return true;

        var connection = data.Connections.SingleOrDefault(p => p.Url == url);

        return connection == null || !connection.UnavailableEndpoints.Contains(endpoint.Value);
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
