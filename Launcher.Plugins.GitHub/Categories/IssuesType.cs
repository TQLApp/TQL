using Launcher.Abstractions;
using Launcher.Plugins.GitHub.Services;
using Launcher.Plugins.GitHub.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.GitHub.Categories;

[RootMatchType]
internal class IssuesType : IMatchType
{
    private readonly ConnectionManager _connectionManager;
    private readonly GitHubApi _api;

    public Guid Id => TypeIds.Issues.Id;

    public IssuesType(ConnectionManager connectionManager, GitHubApi api)
    {
        _connectionManager = connectionManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Id == dto.Id))
            return null;

        return new IssuesMatch(
            MatchUtils.GetMatchLabel("GitHub Issue", _connectionManager, dto.Id),
            dto.Id,
            _api
        );
    }
}
