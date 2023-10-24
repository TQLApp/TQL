using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;

    public Guid Id => TypeIds.Board.Id;

    public BoardType(IconCacheManager iconCacheManager)
    {
        _iconCacheManager = iconCacheManager;
    }

    public IMatch Deserialize(string json)
    {
        return new BoardMatch(JsonSerializer.Deserialize<BoardMatchDto>(json)!, _iconCacheManager);
    }
}
