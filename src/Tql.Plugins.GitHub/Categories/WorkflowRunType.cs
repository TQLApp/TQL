using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class WorkflowRunType(
    IMatchFactory<WorkflowRunMatch, WorkflowRunMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<WorkflowRunMatch, WorkflowRunMatchDto>(factory)
{
    public override Guid Id => TypeIds.WorkflowRun.Id;

    protected override bool IsValid(WorkflowRunMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
