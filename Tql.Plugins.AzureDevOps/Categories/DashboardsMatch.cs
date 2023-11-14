using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class DashboardsMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<DashboardMatch, DashboardMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.DashboardsType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Dashboards;
    public override MatchTypeId TypeId => TypeIds.Dashboards;
    public override string SearchHint => Labels.DashboardsMatch_SearchHint;

    public DashboardsMatch(
        RootItemDto dto,
        ICache<AzureData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<DashboardMatch, DashboardMatchDto> factory
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
            from dashboard in project.Dashboards
            select _factory.Create(
                new DashboardMatchDto(_dto.Url, project.Name, dashboard.Id, dashboard.Name)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
