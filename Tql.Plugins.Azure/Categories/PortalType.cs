using Tql.Abstractions;
using Tql.Plugins.Azure.Services;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Categories;

internal class PortalType : MatchType<PortalMatch, PortalMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Portal.Id;

    public PortalType(
        IMatchFactory<PortalMatch, PortalMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(PortalMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
