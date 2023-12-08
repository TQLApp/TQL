using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class ProjectType(
    IMatchFactory<ProjectMatch, ProjectMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<ProjectMatch, ProjectMatchDto>(factory)
{
    public override Guid Id => TypeIds.Project.Id;

    protected override bool IsValid(ProjectMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
