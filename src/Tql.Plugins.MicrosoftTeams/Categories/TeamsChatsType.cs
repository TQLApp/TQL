using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class TeamsChatsType(
    ConfigurationManager configurationManager,
    IMatchFactory<TeamsChatsMatch, RootItemDto> factory
) : PeopleTypeBase<TeamsChatsMatch, TeamsChatMatch>(configurationManager, factory)
{
    public override Guid Id => TypeIds.TeamsChats.Id;
}
