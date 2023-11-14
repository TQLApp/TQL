using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Plugins.MicrosoftTeams.Support;

namespace Tql.Plugins.MicrosoftTeams.Categories;

[RootMatchType]
internal class TeamsVideosType : PeopleTypeBase<TeamsVideosMatch, TeamsVideoMatch>
{
    public override Guid Id => TypeIds.TeamsVideos.Id;

    public TeamsVideosType(
        ConfigurationManager configurationManager,
        IMatchFactory<TeamsVideosMatch, RootItemDto> factory
    )
        : base(configurationManager, factory) { }
}
