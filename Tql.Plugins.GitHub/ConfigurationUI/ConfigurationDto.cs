using System.Collections.ObjectModel;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.ConfigurationUI;

internal class ConfigurationDto
{
    public ObservableCollection<ConnectionDto> Connections { get; } = new();

    public static ConfigurationDto FromConfiguration(Configuration configuration)
    {
        var result = new ConfigurationDto();

        result.Connections.AddRange(
            configuration.Connections.Select(
                p => new ConnectionDto(p.Id, p.PatToken, p.ProtectedCredentials) { Name = p.Name }
            )
        );

        return result;
    }

    public Configuration ToConfiguration()
    {
        return new Configuration(
            Connections
                .Select(p => new Connection(p.Id, p.Name!, p.PatToken, p.ProtectedCredentials))
                .ToImmutableArray()
        );
    }
}

internal class ConnectionDto
{
    public Guid Id { get; }
    public string? PatToken { get; }
    public string? ProtectedCredentials { get; }
    public string? Name { get; set; }

    public ConnectionDto(Guid id, string? patToken, string? protectedCredentials)
    {
        Id = id;
        PatToken = patToken;
        ProtectedCredentials = protectedCredentials;
    }

    public bool GetIsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
}
