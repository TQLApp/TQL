using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;

namespace Tql.Plugins.Confluence.Data;

internal class ConfluenceCacheManager : ICacheManager<ConfluenceData>
{
    private readonly ConfigurationManager _configurationManager;
    private readonly ConfluenceApi _api;

    public int Version => 1;

    public event EventHandler<CacheExpiredEventArgs>? CacheExpired;

    public ConfluenceCacheManager(ConfigurationManager configurationManager, ConfluenceApi api)
    {
        _configurationManager = configurationManager;
        _api = api;

        configurationManager.Changed += (_, _) => OnCacheExpired(new CacheExpiredEventArgs(true));
    }

    public async Task<ConfluenceData> Create()
    {
        var results = ImmutableArray.CreateBuilder<ConfluenceConnection>();

        foreach (var connection in _configurationManager.Configuration.Connections)
        {
            results.Add(await CreateConnection(connection));
        }

        return new ConfluenceData(results.ToImmutable());
    }

    private async Task<ConfluenceConnection> CreateConnection(Connection connection)
    {
        var client = _api.GetClient(connection);

        var spaces = (
            from space in await client.GetSpaces()
            select new ConfluenceSpace(space.Key, space.Name, space.Links.WebUI, space.Icon.Path)
        ).ToImmutableArray();

        return new ConfluenceConnection(connection.Url, spaces);
    }

    protected virtual void OnCacheExpired(CacheExpiredEventArgs e) => CacheExpired?.Invoke(this, e);
}
