using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class PullRequestsMatch : IssuesMatchBase<PullRequestMatch>
{
    public override MatchTypeId TypeId => TypeIds.PullRequests;

    public PullRequestsMatch(
        RootItemDto dto,
        GitHubApi api,
        ICache<GitHubData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<PullRequestMatch, IssueMatchDto> factory
    )
        : base(dto, api, cache, IssueTypeQualifier.PullRequest, configurationManager, factory) { }
}
