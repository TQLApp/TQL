namespace Tql.Plugins.GitHub.Data;

internal record GitHubData(ImmutableArray<GitHubConnectionData> Connections)
{
    public GitHubConnectionData GetConnection(Guid id) => Connections.Single(p => p.Id == id);
}

internal record GitHubConnectionData(
    Guid Id,
    string UserName,
    ImmutableArray<string> Organizations,
    ImmutableArray<GitHubRepository> Repositories
);

internal record GitHubRepository(
    string Owner,
    string Name,
    string HtmlUrl,
    DateTimeOffset UpdatedAt
);
