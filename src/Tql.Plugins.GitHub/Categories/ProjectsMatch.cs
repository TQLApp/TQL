using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class ProjectsMatch(
    RootItemDto dto,
    ICache<GitHubData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<ProjectMatch, ProjectMatchDto> factory
) : ISearchableMatch, ISerializableMatch
{
    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.ProjectsMatch_Label,
            configurationManager.Configuration,
            dto
        );

    public ImageSource Icon => Images.Project;
    public MatchTypeId TypeId => TypeIds.Projects;
    public string SearchHint => Labels.ProjectsMatch_SearchHint;

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var data = await cache.Get();
        var connection = data.GetConnection(dto.Id);

        if (text.IsWhiteSpace())
            return connection.Projects.OrderByDescending(p => p.UpdatedAt).Select(CreateMatch);

        return context.Filter(connection.Projects.Select(CreateMatch));

        ProjectMatch CreateMatch(GitHubProject project)
        {
            return factory.Create(
                new ProjectMatchDto(
                    dto.Id,
                    project.Number,
                    project.Owner,
                    project.Title,
                    project.Url
                )
            );
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
