using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class IssuesMatch : IssuesMatchBase<IssueMatch>
{
    public override MatchTypeId TypeId => TypeIds.Issues;

    public IssuesMatch(
        RootItemDto dto,
        GitHubApi api,
        ICache<GitHubData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<IssueMatch, IssueMatchDto> factory
    )
        : base(dto, api, cache, IssueTypeQualifier.Issue, configurationManager, factory) { }
}
