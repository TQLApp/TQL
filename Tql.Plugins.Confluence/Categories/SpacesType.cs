using Tql.Abstractions;
using Tql.Plugins.Confluence.Data;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

[RootMatchType]
internal class SpacesType : IMatchType
{
    private readonly ICache<ConfluenceData> _cache;
    private readonly ConfigurationManager _configurationManager;
    private readonly IconCacheManager _iconCacheManager;

    public Guid Id => TypeIds.Spaces.Id;

    public SpacesType(
        ICache<ConfluenceData> cache,
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

        return new SpacesMatch(
            MatchUtils.GetMatchLabel(Labels.SpacesType_Label, configuration, dto.Url),
            dto.Url,
            _cache,
            _iconCacheManager,
            _configurationManager
        );
    }
}
