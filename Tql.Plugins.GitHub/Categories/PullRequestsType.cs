using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class PullRequestsType(
    IMatchFactory<PullRequestsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : IssuesTypeBase<PullRequestsMatch, PullRequestMatch>(factory, configurationManager)
{
    public override Guid Id => TypeIds.PullRequests.Id;
}
