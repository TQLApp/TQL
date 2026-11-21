using Tql.Abstractions;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class BoardsMatch(
    RootItemDto dto,
    ICache<YouTrackData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<BoardMatch, BoardMatchDto> factory
) : CachedMatch<YouTrackData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.BoardsMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Boards;
    public override string SearchHint => Labels.BoardsMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(YouTrackData data)
    {
        return from board in data.GetConnection(dto.Url).AgileBoards
            select factory.Create(new BoardMatchDto(dto.Url, board.Id, board.Name));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
