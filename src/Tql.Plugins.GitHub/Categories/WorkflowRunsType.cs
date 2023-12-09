using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class WorkflowRunsType(
    IMatchFactory<WorkflowRunsMatch, RepositoryItemMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<WorkflowRunsMatch, RepositoryItemMatchDto>(factory)
{
    public override Guid Id => TypeIds.WorkflowRuns.Id;

    protected override bool IsValid(RepositoryItemMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
