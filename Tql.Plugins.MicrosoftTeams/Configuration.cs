namespace Tql.Plugins.MicrosoftTeams;

internal record Configuration(ConfigurationMode Mode, ImmutableArray<string> DirectoryIds)
{
    public static readonly Configuration Empty = new(default, ImmutableArray<string>.Empty);

    public static Configuration FromJson(string? configuration)
    {
        if (configuration == null)
            return Empty;

        return JsonSerializer.Deserialize<Configuration>(configuration)!;
    }

    public string ToJson() => JsonSerializer.Serialize(this);
}

internal enum ConfigurationMode
{
    All,
    Selected
}
