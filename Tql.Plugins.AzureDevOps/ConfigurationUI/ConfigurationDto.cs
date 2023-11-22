using System.Collections.ObjectModel;
using Tql.Plugins.AzureDevOps.Support;

namespace Tql.Plugins.AzureDevOps.ConfigurationUI;

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
                            new ConnectionDto(p.Id)
                            {
                                Name = p.Name,
                                Url = p.Url,
                                ProtectedPATToken = p.ProtectedPATToken
                            }
                    )
            );

        return result;
    }

    public Configuration ToConfiguration()
    {
        return new Configuration(
            Connections
                .Select(p => new Connection(p.Id, p.Name!, p.Url!, p.ProtectedPATToken!))
                .ToImmutableArray()
        );
    }
}

internal class ConnectionDto(Guid id)
{
    public Guid Id { get; } = id;
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string? ProtectedPATToken { get; set; }

    public bool GetIsValid()
    {
        return !string.IsNullOrWhiteSpace(Name)
            && !string.IsNullOrWhiteSpace(Url)
            && Uri.TryCreate(Url, UriKind.Absolute, out _)
            && ProtectedPATToken != null;
    }
}
