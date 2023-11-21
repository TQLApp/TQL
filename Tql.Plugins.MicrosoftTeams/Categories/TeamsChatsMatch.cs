using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal class TeamsChatsMatch(
    RootItemDto dto,
    IPeopleDirectoryManager peopleDirectoryManager,
    ConfigurationManager configurationManager,
    IMatchFactory<TeamsChatMatch, PersonDto> factory
) : PeopleMatchBase<TeamsChatMatch>(dto, peopleDirectoryManager, configurationManager, factory)
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.TeamsChatsMatch_Label,
            configurationManager,
            peopleDirectoryManager,
            dto.Id
        );

    public override ImageSource Icon => Images.Teams;
    public override MatchTypeId TypeId => TypeIds.TeamsChats;
}
