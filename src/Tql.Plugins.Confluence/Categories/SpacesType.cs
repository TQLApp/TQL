using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

[RootMatchType]
internal class SpacesType(
    IMatchFactory<SpacesMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<SpacesMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Spaces.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
