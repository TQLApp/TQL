using Tql.Abstractions;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class SavedSearchesMatch(
    RootItemDto dto,
    ICache<YouTrackData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<SavedSearchMatch, SavedSearchMatchDto> factory
) : CachedMatch<YouTrackData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.SavedSearchesMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.SavedSearches;
    public override MatchTypeId TypeId => TypeIds.SavedSearches;
    public override string SearchHint => Labels.SavedSearchesMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(YouTrackData data)
    {
        return from savedSearch in data.GetConnection(dto.Url).SavedSearches
            select factory.Create(
                new SavedSearchMatchDto(
                    dto.Url,
                    savedSearch.Id,
                    savedSearch.Name,
                    savedSearch.Query
                )
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
