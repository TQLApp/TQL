namespace Tql.Plugins.MicrosoftTeams;

internal record Configuration(ConfigurationMode Mode, ImmutableArray<string> DirectoryIds)
{
    public static readonly Configuration Empty = new(default, ImmutableArray<string>.Empty);
}

internal enum ConfigurationMode
{
    All,
    Selected
}
