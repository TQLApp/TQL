using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class GistsType(
    IMatchFactory<GistsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<GistsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Gists.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Id);
}
