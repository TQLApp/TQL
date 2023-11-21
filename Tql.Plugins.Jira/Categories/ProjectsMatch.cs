using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectsMatch(
    RootItemDto dto,
    ICache<JiraData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<ProjectMatch, ProjectMatchDto> factory
) : CachedMatch<JiraData>(cache), ISerializableMatch
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

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        var projects = data.GetConnection(dto.Url).Projects;

        return from project in projects
            select factory.Create(
                new ProjectMatchDto(dto.Url, project.Key, project.Name, project.AvatarUrl)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
