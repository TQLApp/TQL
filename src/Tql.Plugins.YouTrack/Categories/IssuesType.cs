using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

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
