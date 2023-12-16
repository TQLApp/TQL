using System.Collections.ObjectModel;
using Tql.Plugins.Azure.Support;
using Tql.Utilities;

namespace Tql.Plugins.Azure.ConfigurationUI;

internal class ConfigurationDto
{
    public ObservableCollection<ConnectionDto> Connections { get; } = new();

    public static ConfigurationDto FromConfiguration(Configuration configuration)
    {
        var result = new ConfigurationDto();

        result.Connections.AddRange(
            configuration.Connections.Select(
                p => new ConnectionDto(p.Id) { Name = p.Name, TenantId = p.TenantId }
            )
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

internal class ConnectionDto : DtoBase
{
    public Guid Id { get; }

    public string? Name
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public string? TenantId
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public ConnectionDto(Guid id)
    {
        Id = id;

        AddProperty(nameof(Name), ValidateNotEmpty, CoerceEmptyStringToNull);
        AddProperty(nameof(TenantId), ValidateNotEmpty, CoerceEmptyStringToNull);
    }

    public ConnectionDto Clone() => (ConnectionDto)Clone(new ConnectionDto(Id));
}
