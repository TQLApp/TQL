using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class ProjectItemType(
    IMatchFactory<ProjectItemMatch, ProjectItemMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<ProjectItemMatch, ProjectItemMatchDto>(factory)
{
    public override Guid Id => TypeIds.ProjectItem.Id;

    protected override bool IsValid(ProjectItemMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
