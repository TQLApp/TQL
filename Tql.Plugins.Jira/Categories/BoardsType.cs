using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class BoardsType : IMatchType
{
    private readonly ICache<JiraData> _cache;
    private readonly ConnectionManager _connectionManager;
    private readonly IconCacheManager _iconCacheManager;
    private readonly JiraApi _api;
    private readonly ILogger<BoardsMatch> _matchLogger;

    public Guid Id => TypeIds.Boards.Id;

    public BoardsType(
        ICache<JiraData> cache,
        ConnectionManager connectionManager,
        IconCacheManager iconCacheManager,
        JiraApi api,
        ILogger<BoardsMatch> matchLogger
    )
    {
        _cache = cache;
        _connectionManager = connectionManager;
        _iconCacheManager = iconCacheManager;
        _api = api;
        _matchLogger = matchLogger;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new BoardsMatch(
            MatchUtils.GetMatchLabel("JIRA Board", _connectionManager, dto.Url),
            dto.Url,
            _cache,
            _iconCacheManager,
            _api,
            _matchLogger
        );
    }
}
