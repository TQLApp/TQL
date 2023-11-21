using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class TeamsCallsType(
    ConfigurationManager configurationManager,
    IMatchFactory<TeamsCallsMatch, RootItemDto> factory
) : PeopleTypeBase<TeamsCallsMatch, TeamsCallMatch>(configurationManager, factory)
{
    public override Guid Id => TypeIds.TeamsCalls.Id;
}
