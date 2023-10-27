using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class NewsType : IMatchType
{
    private readonly ICache<AzureData> _cache;
    private readonly ConfigurationManager _configurationManager;
    private readonly AzureWorkItemIconManager _iconManager;

    public Guid Id => TypeIds.News.Id;

    public NewsType(
        ICache<AzureData> cache,
        ConfigurationManager configurationManager,
        AzureWorkItemIconManager iconManager
    )
    {
        _cache = cache;
        _configurationManager = configurationManager;
        _iconManager = iconManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.HasConnection(dto.Url))
            return null;

        return new NewsMatch(
            MatchUtils.GetMatchLabel("Azure New", configuration, dto.Url),
            dto.Url,
            _cache,
            _iconManager
        );
    }
}
