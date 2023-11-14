using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class DashboardsMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<DashboardMatch, DashboardMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.DashboardsMatch_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Dashboards;
    public override MatchTypeId TypeId => TypeIds.Dashboards;
    public override string SearchHint => Labels.DashboardsMatch_SearchHint;

    public DashboardsMatch(
        RootItemDto dto,
        ICache<JiraData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<DashboardMatch, DashboardMatchDto> factory
    )
        : base(cache)
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        return from dashboard in data.GetConnection(_dto.Url).Dashboards
            select _factory.Create(new DashboardMatchDto(_dto.Url, dashboard.Name, dashboard.View));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
