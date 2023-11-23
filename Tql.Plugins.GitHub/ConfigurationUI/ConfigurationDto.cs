using System.Collections.ObjectModel;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

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

internal class ConnectionDto : DtoBase
{
    public Guid Id { get; }

    public string? Name
    {
        get => (string?)GetValue(nameof(Name));
        set => SetValue(nameof(Name), value);
    }

    public string? PatToken { get; }
    public string? ProtectedCredentials { get; }

    public ConnectionDto(Guid id, string? patToken, string? protectedCredentials)
    {
        Id = id;
        PatToken = patToken;
        ProtectedCredentials = protectedCredentials;

        AddProperty(nameof(Name), ValidateNotEmpty, CoerceEmptyStringToNull);
    }

    public ConnectionDto Clone() =>
        (ConnectionDto)Clone(new ConnectionDto(Id, PatToken, ProtectedCredentials));
}
