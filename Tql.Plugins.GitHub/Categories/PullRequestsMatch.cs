using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestsMatch : IssuesMatchBase
{
    public override MatchTypeId TypeId => TypeIds.PullRequests;

    public PullRequestsMatch(string text, Guid id, GitHubApi api)
        : base(text, id, api, IssueTypeQualifier.PullRequest) { }

    protected override IssueMatchBase CreateIssue(IssueMatchDto dto) => new PullRequestMatch(dto);
}
