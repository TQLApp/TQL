using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class IssuesType : IssuesTypeBase<IssuesMatch, IssueMatch>
{
    public override Guid Id => TypeIds.Issues.Id;

    public IssuesType(
        IMatchFactory<IssuesMatch, RootItemDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory, configurationManager) { }
}
