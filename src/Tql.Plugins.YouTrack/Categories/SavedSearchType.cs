using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class SavedSearchType(
    IMatchFactory<SavedSearchMatch, SavedSearchMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<SavedSearchMatch, SavedSearchMatchDto>(factory)
{
    public override Guid Id => TypeIds.SavedSearch.Id;

    protected override bool IsValid(SavedSearchMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
