using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsChatsMatch(
    RootItemDto dto,
    ConfigurationManager configurationManager,
    IMatchFactory<TeamsChatMatch, PersonDto> factory
)
    : PeopleMatchBase<TeamsChatMatch>(
        dto,
        Labels.TeamsChatsMatch_Label,
        configurationManager,
        factory
    )
{
    public override ImageSource Icon => Images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsChats;
}
