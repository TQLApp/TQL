using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

[RootMatchType]
internal class SpacesType : MatchType<SpacesMatch, RootItemDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Spaces.Id;

    public SpacesType(
        IMatchFactory<SpacesMatch, RootItemDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RootItemDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
