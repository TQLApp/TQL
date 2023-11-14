using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectsMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<ProjectMatch, ProjectMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.ProjectsType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Projects;
    public override MatchTypeId TypeId => TypeIds.Projects;

    public ProjectsMatch(
        RootItemDto dto,
        ICache<JiraData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<ProjectMatch, ProjectMatchDto> factory
    )
        : base(cache)
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        var projects = data.GetConnection(_dto.Url).Projects;

        return from project in projects
            select _factory.Create(
                new ProjectMatchDto(_dto.Url, project.Key, project.Name, project.AvatarUrl)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
