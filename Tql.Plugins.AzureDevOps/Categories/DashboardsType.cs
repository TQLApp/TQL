using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class DashboardsType : MatchType<DashboardsMatch, RootItemDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Dashboards.Id;

    public DashboardsType(
        IMatchFactory<DashboardsMatch, RootItemDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RootItemDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
