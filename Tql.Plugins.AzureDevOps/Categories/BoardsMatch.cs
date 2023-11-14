using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BoardsMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<BoardMatch, BoardMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.BoardsType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Boards;

    public BoardsMatch(
        RootItemDto dto,
        ICache<AzureData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<BoardMatch, BoardMatchDto> factory
    )
        : base(cache)
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(_dto.Url).Projects
            from team in project.Teams
            from board in project.Boards
            select _factory.Create(
                new BoardMatchDto(_dto.Url, project.Name, team.Name, board.Name)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
