using Tql.Abstractions;
using Tql.Plugins.Azure.Services;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Categories;

internal class PortalType(
    IMatchFactory<PortalMatch, PortalMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<PortalMatch, PortalMatchDto>(factory)
{
    public override Guid Id => TypeIds.Portal.Id;

    protected override bool IsValid(PortalMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
