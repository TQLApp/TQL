using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SpaceType(
    IMatchFactory<SpaceMatch, SpaceMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<SpaceMatch, SpaceMatchDto>(factory)
{
    public override Guid Id => TypeIds.Space.Id;

    protected override bool IsValid(SpaceMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
