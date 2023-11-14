using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardQuickFilterType : MatchType<BoardQuickFilterMatch, BoardQuickFilterMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.BoardQuickFilter.Id;

    public BoardQuickFilterType(
        IMatchFactory<BoardQuickFilterMatch, BoardQuickFilterMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(BoardQuickFilterMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Board.Url);
}
