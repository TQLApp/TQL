using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

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
