using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class IssuesType(
    IMatchFactory<IssuesMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<IssuesMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Issues.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
