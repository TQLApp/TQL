using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;

    public Guid Id => TypeIds.Project.Id;

    public ProjectType(IconCacheManager iconCacheManager)
    {
        _iconCacheManager = iconCacheManager;
    }

    public IMatch Deserialize(string json)
    {
        return new ProjectMatch(
            JsonSerializer.Deserialize<ProjectMatchDto>(json)!,
            _iconCacheManager
        );
    }
}
