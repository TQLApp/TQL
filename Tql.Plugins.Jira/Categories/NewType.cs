using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class NewType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IconCacheManager _iconCacheManager;
    private readonly ICache<JiraData> _cache;

    public Guid Id => TypeIds.New.Id;

    public NewType(
        ConfigurationManager configurationManager,
        IconCacheManager iconCacheManager,
        ICache<JiraData> cache
    )
    {
        _configurationManager = configurationManager;
        _iconCacheManager = iconCacheManager;
        _cache = cache;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<NewMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new NewMatch(dto, _iconCacheManager, _cache);
    }
}
