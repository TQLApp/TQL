using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType]
internal class ProjectsType(
    IMatchFactory<ProjectsMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<ProjectsMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Projects.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Id);
}
