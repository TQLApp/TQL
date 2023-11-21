using System.Collections.ObjectModel;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.ConfigurationUI;

internal class ConfigurationDto
{
    public ObservableCollection<ConnectionDto> Connections { get; } = new();

    public static ConfigurationDto FromConfiguration(Configuration configuration)
    {
        var result = new ConfigurationDto();

        result
            .Connections
            .AddRange(
                configuration
                    .Connections
                    .Select(
                        p =>
                            new ConnectionDto(p.Id, p.PatToken, p.ProtectedCredentials)
                            {
                                Name = p.Name
                            }
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

internal class ConnectionDto(Guid id, string? patToken, string? protectedCredentials)
{
    public Guid Id { get; } = id;
    public string? PatToken { get; } = patToken;
    public string? ProtectedCredentials { get; } = protectedCredentials;
    public string? Name { get; set; }

    public bool GetIsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
}
