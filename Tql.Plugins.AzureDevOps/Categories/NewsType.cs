using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class NewsType : IMatchType
{
    private readonly ICache<AzureData> _cache;
    private readonly ConnectionManager _connectionManager;
    private readonly AzureWorkItemIconManager _iconManager;

    public Guid Id => TypeIds.News.Id;

    public NewsType(
        ICache<AzureData> cache,
        ConnectionManager connectionManager,
        AzureWorkItemIconManager iconManager
    )
    {
        _cache = cache;
        _connectionManager = connectionManager;
        _iconManager = iconManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new NewsMatch(
            MatchUtils.GetMatchLabel("Azure New", _connectionManager, dto.Url),
            dto.Url,
            _cache,
            _iconManager
        );
    }
}
