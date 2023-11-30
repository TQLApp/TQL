using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class WorkItemType(
    IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<WorkItemMatch, WorkItemMatchDto>(factory)
{
    public override Guid Id => TypeIds.WorkItem.Id;

    protected override bool IsValid(WorkItemMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
