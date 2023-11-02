using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class TeamsCallsType : PeopleTypeBase
{
    public override Guid Id => TypeIds.TeamsCalls.Id;
    protected override string Label => Labels.TeamsCallsType_Label;

    public TeamsCallsType(
        ConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager
    )
        : base(configurationManager, peopleDirectoryManager) { }

    protected override IMatch CreateMatch(string label, IPeopleDirectory directory)
    {
        return new TeamsCallsMatch(label, directory);
    }
}
