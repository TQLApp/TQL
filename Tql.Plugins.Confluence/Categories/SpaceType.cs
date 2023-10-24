using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SpaceType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Space.Id;

    public SpaceType(IconCacheManager iconCacheManager, ConfigurationManager configurationManager)
    {
        _iconCacheManager = iconCacheManager;
        _configurationManager = configurationManager;
    }

    public IMatch Deserialize(string json)
    {
        return new SpaceMatch(
            JsonSerializer.Deserialize<SpaceMatchDto>(json)!,
            _iconCacheManager,
            _configurationManager
        );
    }
}
