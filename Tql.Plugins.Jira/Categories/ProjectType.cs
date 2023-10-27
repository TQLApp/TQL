using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class ProjectType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Project.Id;

    public ProjectType(IconCacheManager iconCacheManager, ConfigurationManager configurationManager)
    {
        _iconCacheManager = iconCacheManager;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<ProjectMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new ProjectMatch(dto, _iconCacheManager, _configurationManager);
    }
}
