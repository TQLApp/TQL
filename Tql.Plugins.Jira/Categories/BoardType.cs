using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class BoardType : MatchType<BoardMatch, BoardMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Board.Id;

    public BoardType(
        IMatchFactory<BoardMatch, BoardMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(BoardMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
