using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SpaceType : IMatchType
{
    private readonly IconCacheManager _iconCacheManager;

    public Guid Id => TypeIds.Space.Id;

    public SpaceType(IconCacheManager iconCacheManager)
    {
        _iconCacheManager = iconCacheManager;
    }

    public IMatch Deserialize(string json)
    {
        return new SpaceMatch(JsonSerializer.Deserialize<SpaceMatchDto>(json)!, _iconCacheManager);
    }
}
