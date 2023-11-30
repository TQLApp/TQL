using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class TeamsVideosType(
    ConfigurationManager configurationManager,
    IMatchFactory<TeamsVideosMatch, RootItemDto> factory
) : PeopleTypeBase<TeamsVideosMatch, TeamsVideoMatch>(configurationManager, factory)
{
    public override Guid Id => TypeIds.TeamsVideos.Id;
}
