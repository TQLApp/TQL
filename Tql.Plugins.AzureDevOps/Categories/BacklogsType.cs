using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class BacklogsType : IMatchType
{
    private readonly ICache<AzureData> _cache;
    private readonly ConnectionManager _connectionManager;

    public Guid Id => TypeIds.Backlogs.Id;

    public BacklogsType(ICache<AzureData> cache, ConnectionManager connectionManager)
    {
        _cache = cache;
        _connectionManager = connectionManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new BacklogsMatch(
            MatchUtils.GetMatchLabel("Azure Backlog", _connectionManager, dto.Url),
            dto.Url,
            _cache
        );
    }
}
