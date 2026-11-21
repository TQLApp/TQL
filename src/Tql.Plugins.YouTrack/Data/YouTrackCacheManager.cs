using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;

namespace Tql.Plugins.YouTrack.Data;

internal class YouTrackCacheManager : ICacheManager<YouTrackData>
{
    private readonly ConfigurationManager _configurationManager;
    private readonly YouTrackApi _api;
    private readonly ILogger<YouTrackCacheManager> _logger;

    public int Version => 1;

    public event EventHandler<CacheInvalidationRequiredEventArgs>? CacheInvalidationRequired;

    public YouTrackCacheManager(
        ConfigurationManager configurationManager,
        YouTrackApi api,
        ILogger<YouTrackCacheManager> logger
    )
    {
        _configurationManager = configurationManager;
        _api = api;
        _logger = logger;

        configurationManager.Changed += (_, _) =>
            OnCacheInvalidationRequired(new CacheInvalidationRequiredEventArgs(true));
    }

    public async Task<YouTrackData> Create()
    {
        var results = ImmutableArray.CreateBuilder<YouTrackConnection>();

        foreach (var connection in _configurationManager.Configuration.Connections)
        {
            results.Add(await CreateConnection(connection));
        }

        return new YouTrackData(results.ToImmutable());
    }

    private async Task<YouTrackConnection> CreateConnection(Connection connection)
    {
        var client = _api.GetClient(connection);
        var unavailableEndpoints = ImmutableArray.CreateBuilder<YouTrackEndpoint>();

        var projects = await Load(
            YouTrackEndpoint.Projects,
            client.GetProjects,
            p => new YouTrackProject(p.Id, p.ShortName, p.Name, p.IconUrl)
        );

        var agileBoards = await Load(
            YouTrackEndpoint.AgileBoards,
            client.GetAgileBoards,
            p => new YouTrackAgileBoard(p.Id, p.Name)
        );

        var savedSearches = await Load(
            YouTrackEndpoint.SavedSearches,
            client.GetSavedQueries,
            p => new YouTrackSavedSearch(p.Id, p.Name, p.Query)
        );

        var dashboards = await Load(
            YouTrackEndpoint.Dashboards,
            client.GetDashboards,
            p => new YouTrackDashboard(p.Id, p.Name)
        );

        return new YouTrackConnection(
            connection.Url,
            projects,
            agileBoards,
            savedSearches,
            dashboards,
            unavailableEndpoints.ToImmutable()
        );

        // A 403 or 404 leaves the endpoint's category out for this connection,
        // rather than failing the rest of the connection's cache. Anything
        // else, such as a 401, fails the cache.
        async Task<ImmutableArray<TResult>> Load<TDto, TResult>(
            YouTrackEndpoint endpoint,
            Func<CancellationToken, Task<ImmutableArray<TDto>>> load,
            Func<TDto, TResult> selector
        )
        {
            try
            {
                return (await load(CancellationToken.None)).Select(selector).ToImmutableArray();
            }
            catch (YouTrackUnavailableException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Leaving out {Endpoint} for connection '{Name}': {Url} answered {StatusCode}",
                    endpoint,
                    connection.Name,
                    ex.RequestUrl,
                    ex.StatusCode
                );

                unavailableEndpoints.Add(endpoint);

                return ImmutableArray<TResult>.Empty;
            }
        }
    }

    protected virtual void OnCacheInvalidationRequired(CacheInvalidationRequiredEventArgs e) =>
        CacheInvalidationRequired?.Invoke(this, e);
}
