using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BoardType(
    IMatchFactory<BoardMatch, BoardMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<BoardMatch, BoardMatchDto>(factory)
{
    public override Guid Id => TypeIds.Board.Id;

    protected override bool IsValid(BoardMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
