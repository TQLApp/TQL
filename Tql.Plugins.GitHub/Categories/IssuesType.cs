using Octokit;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType]
internal class IssuesType : IssuesTypeBase
{
    public override Guid Id => TypeIds.Issues.Id;

    public IssuesType(ConnectionManager connectionManager, GitHubApi api)
        : base(connectionManager, api, IssueTypeQualifier.Issue) { }

    protected override IssuesMatchBase CreateMatch(string text, Guid id, GitHubApi api) =>
        new IssuesMatch(text, id, api);
}
