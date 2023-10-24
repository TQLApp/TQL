using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class DashboardsType : IMatchType
{
    private readonly ICache<JiraData> _cache;
    private readonly ConnectionManager _connectionManager;

    public Guid Id => TypeIds.Dashboards.Id;

    public DashboardsType(ICache<JiraData> cache, ConnectionManager connectionManager)
    {
        _cache = cache;
        _connectionManager = connectionManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new DashboardsMatch(
            MatchUtils.GetMatchLabel("JIRA Dashboard", _connectionManager, dto.Url),
            dto.Url,
            _cache
        );
    }
}
