using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class DashboardsType(
    IMatchFactory<DashboardsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<DashboardsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Dashboards.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
