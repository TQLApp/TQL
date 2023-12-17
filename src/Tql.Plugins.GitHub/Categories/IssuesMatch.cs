using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class IssuesMatch(
    RepositoryItemMatchDto dto,
    GitHubApi api,
    IMatchFactory<IssueMatch, IssueMatchDto> factory,
    IssueType issueType
) : IssuesMatchBase<IssueMatch>(dto, api, IssueTypeQualifier.Issue, factory, issueType)
{
    public override MatchTypeId TypeId => TypeIds.Issues;
    public override string SearchHint => Labels.IssuesMatch_SearchHint;
}
