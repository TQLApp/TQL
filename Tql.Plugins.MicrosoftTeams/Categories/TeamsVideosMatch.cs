using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsVideosMatch(
    RootItemDto dto,
    IPeopleDirectoryManager peopleDirectoryManager,
    ConfigurationManager configurationManager,
    IMatchFactory<TeamsVideoMatch, PersonDto> factory
)
    : PeopleMatchBase<TeamsVideoMatch>(
        dto,
        Labels.TeamsVideosMatch_Label,
        peopleDirectoryManager,
        configurationManager,
        factory
    )
{
    public override ImageSource Icon => Images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsVideos;
}
