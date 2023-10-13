using Launcher.Abstractions;
using Launcher.Plugins.GitHub.Services;
using Octokit;

namespace Launcher.Plugins.GitHub.Categories;

internal class PullRequestsMatch : IssuesMatchBase
{
    public override MatchTypeId TypeId => TypeIds.PullRequests;

    public PullRequestsMatch(string text, Guid id, GitHubApi api)
        : base(text, id, api, IssueTypeQualifier.PullRequest) { }

    protected override IssueMatchBase CreateIssue(IssueMatchDto dto) => new PullRequestMatch(dto);
}
