using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SpaceType : MatchType<SpaceMatch, SpaceMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Space.Id;

    public SpaceType(
        IMatchFactory<SpaceMatch, SpaceMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(SpaceMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
