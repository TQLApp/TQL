using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class NewsType : IMatchType
{
    private readonly ICache<JiraData> _cache;
    private readonly ConfigurationManager _configurationManager;
    private readonly IconCacheManager _iconCacheManager;

    public Guid Id => TypeIds.News.Id;

    public NewsType(
        ICache<JiraData> cache,
        ConfigurationManager configurationManager,
        IconCacheManager iconCacheManager
    )
    {
        _cache = cache;
        _configurationManager = configurationManager;
        _iconCacheManager = iconCacheManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.HasConnection(dto.Url))
            return null;

        return new NewsMatch(
            MatchUtils.GetMatchLabel("JIRA New", configuration, dto.Url),
            dto.Url,
            _cache,
            _iconCacheManager
        );
    }
}
