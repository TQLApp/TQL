using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType]
internal class UsersType : IMatchType
{
    private readonly ConnectionManager _connectionManager;
    private readonly GitHubApi _api;

    public Guid Id => TypeIds.Users.Id;

    public UsersType(ConnectionManager connectionManager, GitHubApi api)
    {
        _connectionManager = connectionManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Id == dto.Id))
            return null;

        return new UsersMatch(
            MatchUtils.GetMatchLabel("GitHub User", _connectionManager, dto),
            dto,
            _api
        );
    }
}
