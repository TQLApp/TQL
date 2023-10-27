using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;
    private readonly ICache<JiraData> _cache;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Board.Id;

    public BoardType(
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
        var dto = JsonSerializer.Deserialize<BoardMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new BoardMatch(dto, _iconCacheManager, _cache, _configurationManager);
    }
}
