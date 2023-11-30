using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class BoardsType(
    IMatchFactory<BoardsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<BoardsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Boards.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
