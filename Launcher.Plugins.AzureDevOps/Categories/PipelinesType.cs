using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Plugins.AzureDevOps.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class PipelinesType : IMatchType
{
    private readonly ICache<AzureData> _cache;
    private readonly ConnectionManager _connectionManager;

    public Guid Id => TypeIds.Pipelines.Id;

    public PipelinesType(ICache<AzureData> cache, ConnectionManager connectionManager)
    {
        _cache = cache;
        _connectionManager = connectionManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new PipelinesMatch(
            MatchUtils.GetMatchLabel("Azure Pipeline", _connectionManager, dto.Url),
            dto.Url,
            _cache
        );
    }
}
