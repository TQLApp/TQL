using Tql.Abstractions;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class DashboardsMatch(
    RootItemDto dto,
    ICache<YouTrackData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<DashboardMatch, DashboardMatchDto> factory
) : CachedMatch<YouTrackData>(cache), ISerializableMatch
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

    protected override IEnumerable<IMatch> Create(YouTrackData data)
    {
        return from dashboard in data.GetConnection(dto.Url).Dashboards
            select factory.Create(new DashboardMatchDto(dto.Url, dashboard.Id, dashboard.Name));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
