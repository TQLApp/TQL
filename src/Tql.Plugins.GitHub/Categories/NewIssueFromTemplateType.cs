using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class NewIssueFromTemplateType(
    IMatchFactory<NewIssueFromTemplateMatch, NewIssueFromTemplateMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<NewIssueFromTemplateMatch, NewIssueFromTemplateMatchDto>(factory)
{
    public override Guid Id => TypeIds.NewIssue.Id;

    protected override bool IsValid(NewIssueFromTemplateMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Id);
}
