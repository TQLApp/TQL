using Launcher.Abstractions;
using Launcher.Plugins.GitHub.Services;
using Octokit;

namespace Launcher.Plugins.GitHub.Categories;

internal class IssuesMatch : IssuesMatchBase
{
    public override MatchTypeId TypeId => TypeIds.Issues;

    public IssuesMatch(string text, Guid id, GitHubApi api)
        : base(text, id, api, IssueTypeQualifier.Issue) { }

    protected override IssueMatchBase CreateIssue(IssueMatchDto dto) => new IssueMatch(dto);
}
