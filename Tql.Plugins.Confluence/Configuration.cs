namespace Tql.Plugins.Confluence;

internal record Configuration(ImmutableArray<Connection> Connections)
{
    public static readonly Configuration Empty = new(ImmutableArray<Connection>.Empty);

    public bool HasConnection(string url) => Connections.Any(p => p.Url == url);
}

internal record Connection(
    Guid Id,
    string Name,
    string Url,
    string? UserName,
    string ProtectedPassword
);
