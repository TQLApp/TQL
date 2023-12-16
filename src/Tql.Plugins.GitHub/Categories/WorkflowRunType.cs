using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class WorkflowRunType(
    IMatchFactory<WorkflowRunMatch, WorkflowRunMatchDto> factory,
    ConfigurationManager configurationManager
) : CachingMatchType<WorkflowRunMatch, WorkflowRunMatchDto, WorkflowRunMatchDto>(factory)
{
    public override Guid Id => TypeIds.WorkflowRun.Id;

    protected override bool IsValid(WorkflowRunMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);

    protected override WorkflowRunMatchDto GetKey(WorkflowRunMatchDto dto)
    {
        return dto with { Status = 0 };
    }
}
