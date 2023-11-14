using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BacklogsMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<BacklogMatch, BacklogMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.BacklogsType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.Backlogs;
    public override string SearchHint => Labels.BacklogsMatch_SearchHint;

    public BacklogsMatch(
        RootItemDto dto,
        ConfigurationManager configurationManager,
        ICache<AzureData> cache,
        IMatchFactory<BacklogMatch, BacklogMatchDto> factory
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
            from backlog in project.Backlogs
            select _factory.Create(
                new BacklogMatchDto(_dto.Url, project.Name, team.Name, backlog.Name)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
