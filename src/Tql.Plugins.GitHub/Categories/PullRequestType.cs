using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestType(
    IMatchFactory<PullRequestMatch, IssueMatchDto> factory,
    ConfigurationManager configurationManager
) : IssueTypeBase<PullRequestMatch>(factory)
{
    public override Guid Id => TypeIds.PullRequest.Id;

    protected override bool IsValid(IssueMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
