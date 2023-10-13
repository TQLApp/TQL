using Launcher.Plugins.GitHub.Services;
using Launcher.Plugins.GitHub.Support;
using Octokit;

namespace Launcher.Plugins.GitHub.Categories;

[RootMatchType]
internal class PullRequestsType : IssuesTypeBase
{
    public override Guid Id => TypeIds.PullRequests.Id;

    public PullRequestsType(ConnectionManager connectionManager, GitHubApi api)
        : base(connectionManager, api, IssueTypeQualifier.PullRequest) { }

    protected override IssuesMatchBase CreateMatch(string text, Guid id, GitHubApi api) =>
        new PullRequestsMatch(text, id, api);
}
