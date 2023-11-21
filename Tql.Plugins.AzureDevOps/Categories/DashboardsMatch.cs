using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class DashboardsMatch(
    RootItemDto dto,
    ICache<AzureData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<DashboardMatch, DashboardMatchDto> factory
) : CachedMatch<AzureData>(cache), ISerializableMatch
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

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(dto.Url).Projects
            from dashboard in project.Dashboards
            select factory.Create(
                new DashboardMatchDto(dto.Url, project.Name, dashboard.Id, dashboard.Name)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
