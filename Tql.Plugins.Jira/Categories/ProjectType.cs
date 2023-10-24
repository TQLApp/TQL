using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;
    private readonly JiraApi _api;

    public Guid Id => TypeIds.Project.Id;

    public ProjectType(IconCacheManager iconCacheManager, JiraApi api)
    {
        _iconCacheManager = iconCacheManager;
        _api = api;
    }

    public IMatch Deserialize(string json)
    {
        return new ProjectMatch(
            JsonSerializer.Deserialize<ProjectMatchDto>(json)!,
            _iconCacheManager,
            _api
        );
    }
}
