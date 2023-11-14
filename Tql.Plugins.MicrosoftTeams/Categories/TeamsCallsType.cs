using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class TeamsCallsType : PeopleTypeBase<TeamsCallsMatch, TeamsCallMatch>
{
    public override Guid Id => TypeIds.TeamsCalls.Id;

    public TeamsCallsType(
        ConfigurationManager configurationManager,
        IMatchFactory<TeamsCallsMatch, RootItemDto> factory
    )
        : base(configurationManager, factory) { }
}
