using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class IssuesType : IMatchType
{
    private readonly JiraApi _api;
    private readonly ConfigurationManager _configurationManager;
    private readonly IconCacheManager _iconCacheManager;

    public Guid Id => TypeIds.Issues.Id;

    public IssuesType(
        JiraApi api,
        ConfigurationManager configurationManager,
        IconCacheManager iconCacheManager
    )
    {
        _api = api;
        _configurationManager = configurationManager;
        _iconCacheManager = iconCacheManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new IssuesMatch(
            MatchUtils.GetMatchLabel("JIRA Issue", configuration, dto.Url),
            dto.Url,
            _api,
            _iconCacheManager
        );
    }
}
