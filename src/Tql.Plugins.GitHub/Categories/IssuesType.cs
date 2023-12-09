using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;

namespace Tql.Plugins.GitHub.Categories;

internal class IssuesType(
    IMatchFactory<IssuesMatch, RepositoryItemMatchDto> factory,
    ConfigurationManager configurationManager
) : IssuesTypeBase<IssuesMatch, IssueMatch>(factory, configurationManager)
{
    public override Guid Id => TypeIds.Issues.Id;
}
