using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class DashboardsMatch(
    RootItemDto dto,
    ICache<JiraData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<DashboardMatch, DashboardMatchDto> factory
) : CachedMatch<JiraData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.DashboardsMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Dashboards;
    public override MatchTypeId TypeId => TypeIds.Dashboards;
    public override string SearchHint => Labels.DashboardsMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        return from dashboard in data.GetConnection(dto.Url).Dashboards
            select factory.Create(new DashboardMatchDto(dto.Url, dashboard.Name, dashboard.View));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
