using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class IssuesType(
    IMatchFactory<IssuesMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : IssuesTypeBase<IssuesMatch, IssueMatch>(factory, configurationManager)
{
    public override Guid Id => TypeIds.Issues.Id;
}
