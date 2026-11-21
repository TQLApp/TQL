using Tql.Abstractions;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

[RootMatchType(YouTrackEndpoint.Projects)]
internal class ProjectsType(
    IMatchFactory<ProjectsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<ProjectsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Projects.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
