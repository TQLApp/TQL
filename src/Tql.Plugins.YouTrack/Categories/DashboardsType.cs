using Tql.Abstractions;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

[RootMatchType(YouTrackEndpoint.Dashboards)]
internal class DashboardsType(
    IMatchFactory<DashboardsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<DashboardsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Dashboards.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
