using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsVideosMatch(
    RootItemDto dto,
    IPeopleDirectoryManager peopleDirectoryManager,
    ConfigurationManager configurationManager,
    IMatchFactory<TeamsVideoMatch, PersonDto> factory
) : PeopleMatchBase<TeamsVideoMatch>(dto, peopleDirectoryManager, configurationManager, factory)
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.TeamsVideosMatch_Label,
            configurationManager,
            peopleDirectoryManager,
            dto.Id
        );

    public override ImageSource Icon => Images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsVideos;
}
