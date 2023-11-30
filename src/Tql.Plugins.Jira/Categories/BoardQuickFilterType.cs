using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardQuickFilterType(
    IMatchFactory<BoardQuickFilterMatch, BoardQuickFilterMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<BoardQuickFilterMatch, BoardQuickFilterMatchDto>(factory)
{
    public override Guid Id => TypeIds.BoardQuickFilter.Id;

    protected override bool IsValid(BoardQuickFilterMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Board.Url);
}
