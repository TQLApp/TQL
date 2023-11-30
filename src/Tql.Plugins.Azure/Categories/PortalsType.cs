using Tql.Abstractions;
using Tql.Plugins.Azure.Services;
using Tql.Plugins.Azure.Support;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Categories;

[RootMatchType]
internal class PortalsType(
    IMatchFactory<PortalsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<PortalsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Portals.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Id);
}
