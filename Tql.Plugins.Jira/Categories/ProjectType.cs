using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectType : MatchType<ProjectMatch, ProjectMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Project.Id;

    public ProjectType(
        IMatchFactory<ProjectMatch, ProjectMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(ProjectMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
