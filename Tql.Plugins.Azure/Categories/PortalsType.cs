using Tql.Abstractions;
using Tql.Plugins.Azure.Services;
using Tql.Plugins.Azure.Support;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Categories;

[RootMatchType]
internal class PortalsType : MatchType<PortalsMatch, RootItemDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Portals.Id;

    public PortalsType(
        IMatchFactory<PortalsMatch, RootItemDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RootItemDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Id);
}
