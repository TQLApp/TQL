namespace Launcher.Plugins.AzureDevOps.ConfigurationUI;

internal class ConfigurationDto
{
    public List<ConnectionDto> Connections { get; } = new();

    public bool IsValid => Connections.All(p => p.GetIsValid());

    public static ConfigurationDto FromConfiguration(Configuration configuration)
    {
        var result = new ConfigurationDto();

        result.Connections.AddRange(
            configuration.Connections.Select(p => new ConnectionDto { Name = p.Name, Url = p.Url })
        );

        return result;
    }

    public Configuration ToConfiguration()
    {
        return new Configuration(
            Connections.Select(p => new Connection(p.Name!, p.Url!)).ToImmutableArray()
        );
    }
}

internal class ConnectionDto
{
    public string? Name { get; set; }
    public string? Url { get; set; }

    public bool GetIsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Url)
            && Uri.TryCreate(Url, UriKind.Absolute, out _);
    }
}
