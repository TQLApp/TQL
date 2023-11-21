using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class WorkItemsType(
    IMatchFactory<WorkItemsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<WorkItemsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.WorkItems.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
