using Launcher.Abstractions;
using Launcher.Plugins.GitHub.Services;
using Launcher.Plugins.GitHub.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.GitHub.Categories;

[RootMatchType]
internal class RepositoriesType : IMatchType
{
    private readonly ConnectionManager _connectionManager;
    private readonly GitHubApi _api;

    public Guid Id => TypeIds.Repositories.Id;

    public RepositoriesType(ConnectionManager connectionManager, GitHubApi api)
    {
        _connectionManager = connectionManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Id == dto.Id))
            return null;

        return new RepositoriesMatch(
            MatchUtils.GetMatchLabel("GitHub Repository", _connectionManager, dto.Id),
            dto.Id,
            _api
        );
    }
}
