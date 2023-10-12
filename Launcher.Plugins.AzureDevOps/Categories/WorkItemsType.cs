using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Plugins.AzureDevOps.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class WorkItemsType : IMatchType
{
    private readonly ICache<AzureData> _cache;
    private readonly ConnectionManager _connectionManager;
    private readonly AzureDevOpsApi _api;

    public Guid Id => TypeIds.WorkItems.Id;

    public WorkItemsType(
        ICache<AzureData> cache,
        ConnectionManager connectionManager,
        AzureDevOpsApi api
    )
    {
        _cache = cache;
        _connectionManager = connectionManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new WorkItemsMatch(
            MatchUtils.GetMatchLabel("Azure Work Item", _connectionManager, dto.Url),
            dto.Url,
            _cache,
            _api
        );
    }
}
