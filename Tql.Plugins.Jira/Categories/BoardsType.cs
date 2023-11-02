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
    private readonly ConfigurationManager _configurationManager;
    private readonly IconCacheManager _iconCacheManager;
    private readonly ILogger<BoardsMatch> _matchLogger;

    public Guid Id => TypeIds.Boards.Id;

    public BoardsType(
        ICache<JiraData> cache,
        ConfigurationManager configurationManager,
        IconCacheManager iconCacheManager,
        ILogger<BoardsMatch> matchLogger
    )
    {
        _cache = cache;
        _configurationManager = configurationManager;
        _iconCacheManager = iconCacheManager;
        _matchLogger = matchLogger;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.HasConnection(dto.Url))
            return null;

        return new BoardsMatch(
            MatchUtils.GetMatchLabel(Labels.BoardsType_Label, configuration, dto.Url),
            dto.Url,
            _cache,
            _iconCacheManager,
            _configurationManager,
            _matchLogger
        );
    }
}
