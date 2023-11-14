using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class PullRequestsType : IssuesTypeBase<PullRequestsMatch, PullRequestMatch>
{
    public override Guid Id => TypeIds.PullRequests.Id;

    public PullRequestsType(
        IMatchFactory<PullRequestsMatch, RootItemDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory, configurationManager) { }
}
