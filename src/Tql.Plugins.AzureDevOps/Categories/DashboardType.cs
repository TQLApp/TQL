using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class DashboardType(
    IMatchFactory<DashboardMatch, DashboardMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<DashboardMatch, DashboardMatchDto>(factory)
{
    public override Guid Id => TypeIds.Dashboard.Id;

    protected override bool IsValid(DashboardMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
