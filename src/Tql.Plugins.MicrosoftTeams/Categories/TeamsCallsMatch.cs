using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsCallsMatch(
    RootItemDto dto,
    ConfigurationManager configurationManager,
    IMatchFactory<TeamsCallMatch, PersonDto> factory
)
    : PeopleMatchBase<TeamsCallMatch>(
        dto,
        Labels.TeamsCallsMatch_Label,
        configurationManager,
        factory
    )
{
    public override ImageSource Icon => Images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsCalls;
}
