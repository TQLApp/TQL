using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Plugins.AzureDevOps.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

[RootMatchType]
internal class TeamsCallsType : IMatchType
{
    private readonly Images _images;
    private readonly ConnectionManager _connectionManager;
    private readonly AzureDevOpsApi _api;

    public Guid Id => TypeIds.TeamsCalls.Id;

    public TeamsCallsType(Images images, ConnectionManager connectionManager, AzureDevOpsApi api)
    {
        _images = images;
        _connectionManager = connectionManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new TeamsCallsMatch(
            MatchUtils.GetMatchLabel("Teams Call", _connectionManager, dto.Url),
            dto.Url,
            _api,
            _images
        );
    }
}
