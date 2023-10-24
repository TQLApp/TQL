using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class ProjectsType : IMatchType
{
    private readonly ICache<JiraData> _cache;
    private readonly ConfigurationManager _configurationManager;
    private readonly IconCacheManager _iconCacheManager;
    private readonly JiraApi _api;
    private readonly ILogger<ProjectsMatch> _matchLogger;

    public Guid Id => TypeIds.Projects.Id;

    public ProjectsType(
        ICache<JiraData> cache,
        ConfigurationManager configurationManager,
        IconCacheManager iconCacheManager,
        JiraApi api,
        ILogger<ProjectsMatch> matchLogger
    )
    {
        _cache = cache;
        _configurationManager = configurationManager;
        _iconCacheManager = iconCacheManager;
        _api = api;
        _matchLogger = matchLogger;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new ProjectsMatch(
            MatchUtils.GetMatchLabel("JIRA Project", configuration, dto.Url),
            dto.Url,
            _cache,
            _iconCacheManager,
            _api,
            _matchLogger
        );
    }
}
