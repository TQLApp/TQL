using Tql.Abstractions;
using Tql.Plugins.Confluence.Data;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SpacesMatch(
    RootItemDto dto,
    ICache<ConfluenceData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<SpaceMatch, SpaceMatchDto> factory
) : CachedMatch<ConfluenceData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.SpacesMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Confluence;
    public override MatchTypeId TypeId => TypeIds.Spaces;
    public override string SearchHint => Labels.SpacesMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(ConfluenceData data)
    {
        // Download the project avatars in the background.

        var spaces = data.GetConnection(dto.Url).Spaces;

        return from space in spaces
            select factory.Create(
                new SpaceMatchDto(dto.Url, space.Key, space.Name, space.Url, space.Icon)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
