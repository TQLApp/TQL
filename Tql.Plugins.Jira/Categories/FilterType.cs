using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class FilterType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;
    private readonly IconCacheManager _iconCacheManager;

    public Guid Id => TypeIds.Filter.Id;

    public FilterType(ConfigurationManager configurationManager, IconCacheManager iconCacheManager)
    {
        _configurationManager = configurationManager;
        _iconCacheManager = iconCacheManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<FilterMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new FilterMatch(dto, _iconCacheManager, _configurationManager);
    }
}
