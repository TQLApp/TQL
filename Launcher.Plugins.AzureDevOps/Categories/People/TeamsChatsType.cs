using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Plugins.AzureDevOps.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories.People;

[RootMatchType]
internal class TeamsChatsType : IMatchType
{
    private readonly ConnectionManager _connectionManager;
    private readonly AzureDevOpsApi _api;

    public Guid Id => TypeIds.TeamsChats.Id;

    public TeamsChatsType(ConnectionManager connectionManager, AzureDevOpsApi api)
    {
        _connectionManager = connectionManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new TeamsChatsMatch(
            MatchUtils.GetMatchLabel("Teams Chat", _connectionManager, dto.Url),
            dto.Url,
            _api
        );
    }
}
