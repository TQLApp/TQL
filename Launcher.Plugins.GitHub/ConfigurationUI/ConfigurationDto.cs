using System.Collections.ObjectModel;
using Launcher.Plugins.GitHub.Support;

namespace Launcher.Plugins.GitHub.ConfigurationUI;

internal class ConfigurationDto
{
    public ObservableCollection<ConnectionDto> Connections { get; } = new();

    public static ConfigurationDto FromConfiguration(Configuration configuration)
    {
        var result = new ConfigurationDto();

        result.Connections.AddRange(
            configuration.Connections.Select(p => new ConnectionDto(p.Id) { Name = p.Name })
        );

        return result;
    }

    public Configuration ToConfiguration()
    {
        return new Configuration(
            Connections.Select(p => new Connection(p.Id, p.Name!)).ToImmutableArray()
        );
    }
}

internal class ConnectionDto
{
    public Guid Id { get; }
    public string? Name { get; set; }

    public ConnectionDto(Guid id)
    {
        Id = id;
    }

    public bool GetIsValid()
    {
        return !string.IsNullOrWhiteSpace(Name);
    }
}
