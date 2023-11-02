using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class TeamsChatsType : PeopleTypeBase
{
    public override Guid Id => TypeIds.TeamsChats.Id;
    protected override string Label => Labels.TeamsChatsType_Label;

    public TeamsChatsType(
        ConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager
    )
        : base(configurationManager, peopleDirectoryManager) { }

    protected override IMatch CreateMatch(string label, IPeopleDirectory directory)
    {
        return new TeamsChatsMatch(label, directory);
    }
}
