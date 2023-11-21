using System.Collections.ObjectModel;
using Tql.Plugins.Azure.Support;

namespace Tql.Plugins.Azure.ConfigurationUI;

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
                    .Select(p => new ConnectionDto(p.Id) { Name = p.Name, TenantId = p.TenantId })
            );

        return result;
    }

    public Configuration ToConfiguration()
    {
        return new Configuration(
            Connections.Select(p => new Connection(p.Id, p.Name!, p.TenantId!)).ToImmutableArray()
        );
    }
}

internal class ConnectionDto(Guid id)
{
    public Guid Id { get; } = id;
    public string? Name { get; set; }
    public string? TenantId { get; set; }

    public bool GetIsValid()
    {
        return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(TenantId);
    }
}
