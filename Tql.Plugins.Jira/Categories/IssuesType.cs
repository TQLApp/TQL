using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class IssuesType : IMatchType
{
    private readonly JiraApi _api;
    private readonly ConnectionManager _connectionManager;
    private readonly IconCacheManager _iconCacheManager;

    public Guid Id => TypeIds.Issues.Id;

    public IssuesType(
        JiraApi api,
        ConnectionManager connectionManager,
        IconCacheManager iconCacheManager
    )
    {
        _api = api;
        _connectionManager = connectionManager;
        _iconCacheManager = iconCacheManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new IssuesMatch(
            MatchUtils.GetMatchLabel("JIRA Issue", _connectionManager, dto.Url),
            dto.Url,
            _api,
            _iconCacheManager
        );
    }
}
