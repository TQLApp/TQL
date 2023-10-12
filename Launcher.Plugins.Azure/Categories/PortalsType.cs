using Launcher.Abstractions;
using Launcher.Plugins.Azure.Services;
using Launcher.Plugins.Azure.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.Azure.Categories;

[RootMatchType]
internal class PortalsType : IMatchType
{
    private readonly ConnectionManager _connectionManager;
    private readonly AzureApi _api;

    public Guid Id => TypeIds.Portals.Id;

    public PortalsType(ConnectionManager connectionManager, AzureApi api)
    {
        _connectionManager = connectionManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Id == dto.Id))
            return null;

        return new PortalsMatch(
            MatchUtils.GetMatchLabel("Azure Portal", _connectionManager, dto.Id),
            dto.Id,
            _api
        );
    }
}
