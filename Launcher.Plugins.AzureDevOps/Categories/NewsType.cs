using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Plugins.AzureDevOps.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class NewsType : IMatchType
{
    private readonly Images _images;
    private readonly ICache<AzureData> _cache;
    private readonly ConnectionManager _connectionManager;

    public Guid Id => TypeIds.News.Id;

    public NewsType(Images images, ICache<AzureData> cache, ConnectionManager connectionManager)
    {
        _images = images;
        _cache = cache;
        _connectionManager = connectionManager;
    }

    public IMatch Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        return new NewsMatch(
            MatchUtils.GetMatchLabel("Azure New", _connectionManager, dto.Url),
            _images,
            dto.Url,
            _cache
        );
    }
}
