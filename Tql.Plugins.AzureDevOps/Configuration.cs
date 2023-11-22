namespace Tql.Plugins.AzureDevOps;

internal record Configuration(ImmutableArray<Connection> Connections)
{
    public static readonly Configuration Empty = new(ImmutableArray<Connection>.Empty);

    public bool HasConnection(string url)
    {
        return Connections.Any(p => p.Url == url);
    }
}

internal record Connection(Guid Id, string Name, string Url, string ProtectedPATToken);
