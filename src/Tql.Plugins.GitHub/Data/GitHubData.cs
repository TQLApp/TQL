namespace Tql.Plugins.GitHub.Data;

internal record GitHubData(ImmutableArray<GitHubConnectionData> Connections);

internal record GitHubConnectionData(
    Guid Id,
    string UserName,
    ImmutableArray<string> Organizations
);
