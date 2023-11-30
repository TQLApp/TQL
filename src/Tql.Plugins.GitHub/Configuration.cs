namespace Tql.Plugins.GitHub;

internal record Configuration(ImmutableArray<Connection> Connections)
{
    public static readonly Configuration Empty = new(ImmutableArray<Connection>.Empty);

    public bool HasConnection(Guid id) => Connections.Any(p => p.Id == id);
}

internal record Connection(Guid Id, string Name, string? PatToken, string? ProtectedCredentials);
