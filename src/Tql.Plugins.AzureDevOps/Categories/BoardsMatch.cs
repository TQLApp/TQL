using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BoardsMatch(
    RootItemDto dto,
    ICache<AzureData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<BoardMatch, BoardMatchDto> factory
) : CachedMatch<AzureData>(cache), ISerializableMatch
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

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(dto.Url).Projects
            from team in project.Teams
            from board in project.Boards
            select factory.Create(new BoardMatchDto(dto.Url, project.Name, team.Name, board.Name));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
