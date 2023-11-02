using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class TeamsVideosType : PeopleTypeBase
{
    public override Guid Id => TypeIds.TeamsVideos.Id;
    protected override string Label => Labels.TeamsVideosType_Label;

    public TeamsVideosType(
        ConfigurationManager configurationManager,
        IPeopleDirectoryManager peopleDirectoryManager
    )
        : base(configurationManager, peopleDirectoryManager) { }

    protected override IMatch CreateMatch(string label, IPeopleDirectory directory)
    {
        return new TeamsVideosMatch(label, directory);
    }
}
