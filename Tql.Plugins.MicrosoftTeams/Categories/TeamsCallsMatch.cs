using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsCallsMatch(
    RootItemDto dto,
    IPeopleDirectoryManager peopleDirectoryManager,
    ConfigurationManager configurationManager,
    IMatchFactory<TeamsCallMatch, PersonDto> factory
) : PeopleMatchBase<TeamsCallMatch>(dto, peopleDirectoryManager, configurationManager, factory)
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.TeamsCallsMatch_Label,
            configurationManager,
            peopleDirectoryManager,
            dto.Id
        );

    public override ImageSource Icon => Images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsCalls;
}
