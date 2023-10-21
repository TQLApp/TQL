using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class PullRequestsType : IssuesTypeBase
{
    public override Guid Id => TypeIds.PullRequests.Id;

    public PullRequestsType(
        ConnectionManager connectionManager,
        GitHubApi api,
        ICache<GitHubData> cache
    )
        : base(connectionManager, api, cache, IssueTypeQualifier.PullRequest) { }

    protected override IssuesMatchBase CreateMatch(
        string text,
        RootItemDto dto,
        GitHubApi api,
        ICache<GitHubData> cache
    ) => new PullRequestsMatch(text, dto, api, cache);
}
