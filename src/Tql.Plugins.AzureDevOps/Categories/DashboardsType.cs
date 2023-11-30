using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

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
