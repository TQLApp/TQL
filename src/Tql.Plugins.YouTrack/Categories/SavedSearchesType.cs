using Tql.Abstractions;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

[RootMatchType(YouTrackEndpoint.SavedSearches)]
internal class SavedSearchesType(
    IMatchFactory<SavedSearchesMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<SavedSearchesMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.SavedSearches.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
