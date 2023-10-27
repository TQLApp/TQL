using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardQuickFilterType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;
    private readonly ICache<JiraData> _cache;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.BoardQuickFilter.Id;

    public BoardQuickFilterType(
        IconCacheManager iconCacheManager,
        ICache<JiraData> cache,
        ConfigurationManager configurationManager
    )
    {
        _iconCacheManager = iconCacheManager;
        _cache = cache;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<BoardQuickFilterMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Board.Url))
            return null;

        return new BoardQuickFilterMatch(dto, _iconCacheManager, _cache, _configurationManager);
    }
}
