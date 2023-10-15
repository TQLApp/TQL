﻿namespace Tql.Plugins.Azure;

internal record Configuration(ImmutableArray<Connection> Connections)
{
    public static Configuration FromJson(string? configuration)
    {
        if (configuration == null)
            return new Configuration(ImmutableArray<Connection>.Empty);

        return JsonSerializer.Deserialize<Configuration>(configuration)!;
    }

    public string ToJson() => JsonSerializer.Serialize(this);
}

internal record Connection(Guid Id, string Name);