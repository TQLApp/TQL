using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class IssuesMatch(
    RepositoryItemMatchDto dto,
    GitHubApi api,
    ICache<GitHubData> cache,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : IssuesMatchBase<IssueMatch>(dto, api, cache, IssueTypeQualifier.Issue, factory)
{
    public override MatchTypeId TypeId => TypeIds.Issues;
    public override string SearchHint => Labels.IssuesMatch_SearchHint;
}
