using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class IssuesType : IssuesTypeBase
{
    public override Guid Id => TypeIds.Issues.Id;

    public IssuesType(
        ConfigurationManager configurationManager,
        GitHubApi api,
        ICache<GitHubData> cache
    )
        : base(configurationManager, api, cache, IssueTypeQualifier.Issue) { }

    protected override IssuesMatchBase CreateMatch(
        string text,
        RootItemDto dto,
        GitHubApi api,
        ICache<GitHubData> cache
    ) => new IssuesMatch(text, dto, api, cache);
}
