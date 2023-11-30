using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class ProjectsType(
    IMatchFactory<ProjectsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<ProjectsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Projects.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
