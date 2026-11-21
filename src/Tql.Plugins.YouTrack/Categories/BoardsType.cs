using Tql.Abstractions;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

[RootMatchType(YouTrackEndpoint.AgileBoards)]
internal class BoardsType(
    IMatchFactory<BoardsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<BoardsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Boards.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
