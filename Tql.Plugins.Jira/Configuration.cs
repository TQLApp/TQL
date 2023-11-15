namespace Tql.Plugins.Jira;

internal record Configuration(ImmutableArray<Connection> Connections)
{
    public static readonly Configuration Empty = new(ImmutableArray<Connection>.Empty);

    public static Configuration FromJson(string? configuration)
    {
        if (configuration == null)
            return Empty;

        return JsonSerializer.Deserialize<Configuration>(configuration)!;
    }

    public string ToJson() => JsonSerializer.Serialize(this);

    public bool HasConnection(string url) => Connections.Any(p => p.Url == url);
}

internal record Connection(
    Guid Id,
    string Name,
    string Url,
    string? UserName,
    string ProtectedPassword
);
