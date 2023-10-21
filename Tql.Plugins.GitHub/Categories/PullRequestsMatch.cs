using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestsMatch : IssuesMatchBase
{
    public override MatchTypeId TypeId => TypeIds.PullRequests;

    public PullRequestsMatch(string text, RootItemDto dto, GitHubApi api, ICache<GitHubData> cache)
        : base(text, dto, api, cache, IssueTypeQualifier.PullRequest) { }

    protected override IssueMatchBase CreateIssue(IssueMatchDto dto) => new PullRequestMatch(dto);
}
