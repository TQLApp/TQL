using Tql.Abstractions;
using Tql.Plugins.YouTrack.Data;
using Tql.Plugins.YouTrack.Services;
using Tql.Plugins.YouTrack.Support;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class ProjectsMatch(
    RootItemDto dto,
    ICache<YouTrackData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<ProjectMatch, ProjectMatchDto> factory
) : CachedMatch<YouTrackData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.ProjectsMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Projects;
    public override MatchTypeId TypeId => TypeIds.Projects;
    public override string SearchHint => Labels.ProjectsMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(YouTrackData data)
    {
        return from project in data.GetConnection(dto.Url).Projects
            select factory.Create(
                new ProjectMatchDto(dto.Url, project.ShortName, project.Name, project.IconUrl)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
