using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestsMatch(
    RootItemDto dto,
    GitHubApi api,
    ICache<GitHubData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<PullRequestMatch, IssueMatchDto> factory
)
    : IssuesMatchBase<PullRequestMatch>(
        dto,
        api,
        cache,
        IssueTypeQualifier.PullRequest,
        configurationManager,
        factory
    )
{
    public override MatchTypeId TypeId => TypeIds.PullRequests;
    public override string SearchHint => Labels.PullRequestsMatch_SearchHint;
}
