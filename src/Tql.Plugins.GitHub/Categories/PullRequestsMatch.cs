using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestsMatch(
    RepositoryItemMatchDto dto,
    GitHubApi api,
    ICache<GitHubData> cache,
    IMatchFactory<PullRequestMatch, IssueMatchDto> factory
) : IssuesMatchBase<PullRequestMatch>(dto, api, cache, IssueTypeQualifier.PullRequest, factory)
{
    public override MatchTypeId TypeId => TypeIds.PullRequests;
    public override string SearchHint => Labels.PullRequestsMatch_SearchHint;
}
