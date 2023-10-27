using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class DashboardsType : IMatchType
{
    private readonly ICache<JiraData> _cache;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Dashboards.Id;

    public DashboardsType(ICache<JiraData> cache, ConfigurationManager configurationManager)
    {
        _cache = cache;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.HasConnection(dto.Url))
            return null;

        return new DashboardsMatch(
            MatchUtils.GetMatchLabel("JIRA Dashboard", configuration, dto.Url),
            dto.Url,
            _cache
        );
    }
}
