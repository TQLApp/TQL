using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestsType(
    IMatchFactory<PullRequestsMatch, RepositoryItemMatchDto> factory,
    ConfigurationManager configurationManager
) : IssuesTypeBase<PullRequestsMatch, PullRequestMatch>(factory, configurationManager)
{
    public override Guid Id => TypeIds.PullRequests.Id;
}
