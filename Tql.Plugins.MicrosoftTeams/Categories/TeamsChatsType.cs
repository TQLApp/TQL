using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class TeamsChatsType : PeopleTypeBase<TeamsChatsMatch, TeamsChatMatch>
{
    public override Guid Id => TypeIds.TeamsChats.Id;

    public TeamsChatsType(
        ConfigurationManager configurationManager,
        IMatchFactory<TeamsChatsMatch, RootItemDto> factory
    )
        : base(configurationManager, factory) { }
}
