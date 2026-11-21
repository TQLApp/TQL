using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class IssueType(
    IMatchFactory<IssueMatch, IssueMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<IssueMatch, IssueMatchDto>(factory)
{
    public override Guid Id => TypeIds.Issue.Id;

    protected override bool IsValid(IssueMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
