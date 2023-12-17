using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class IssueType(
    IMatchFactory<IssueMatch, IssueMatchDto> factory,
    ConfigurationManager configurationManager
) : IssueTypeBase<IssueMatch>(factory)
{
    public override Guid Id => TypeIds.Issue.Id;

    protected override bool IsValid(IssueMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
