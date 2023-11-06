using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class IssuesMatch : IssuesMatchBase
{
    public override MatchTypeId TypeId => TypeIds.Issues;

    public IssuesMatch(string text, RootItemDto dto, GitHubApi api, ICache<GitHubData> cache)
        : base(text, dto, api, cache, IssueTypeQualifier.Issue) { }

    protected override IssueMatchBase CreateIssue(IssueMatchDto dto) => new IssueMatch(dto);
}
