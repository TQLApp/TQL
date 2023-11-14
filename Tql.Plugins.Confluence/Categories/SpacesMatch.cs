using Tql.Abstractions;
using Tql.Plugins.Confluence.Data;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SpacesMatch : CachedMatch<ConfluenceData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<SpaceMatch, SpaceMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.SpacesMatch_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Confluence;
    public override MatchTypeId TypeId => TypeIds.Spaces;
    public override string SearchHint => Labels.SpacesMatch_SearchHint;

    public SpacesMatch(
        RootItemDto dto,
        ICache<ConfluenceData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<SpaceMatch, SpaceMatchDto> factory
    )
        : base(cache)
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    protected override IEnumerable<IMatch> Create(ConfluenceData data)
    {
        // Download the project avatars in the background.

        var spaces = data.GetConnection(_dto.Url).Spaces;

        return from space in spaces
            select _factory.Create(
                new SpaceMatchDto(_dto.Url, space.Key, space.Name, space.Url, space.Icon)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
