using System.Collections.ObjectModel;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.ConfigurationUI;

internal class ConfigurationDto
{
    public ObservableCollection<ConnectionDto> Connections { get; } = new();

    public static ConfigurationDto FromConfiguration(
        Configuration configuration,
        IEncryption encryption
    )
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
                                PATToken = encryption.DecryptString(p.ProtectedPATToken)
                            }
                    )
            );

        return result;
    }

    public Configuration ToConfiguration(IEncryption encryption)
    {
        return new Configuration(
            Connections
                .Select(
                    p =>
                        new Connection(p.Id, p.Name!, p.Url!, encryption.EncryptString(p.PATToken)!)
                )
                .ToImmutableArray()
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

    public string? Url
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public string? PATToken
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public ConnectionDto(Guid id)
    {
        Id = id;

        AddProperty(nameof(Name), ValidateNotEmpty, CoerceEmptyStringToNull);
        AddProperty(
            nameof(Url),
            p => ValidateNotEmpty(p) ?? ValidateUrl(p),
            p => CoerceUrlEndsInSlash(CoerceEmptyStringToNull(p))
        );
        AddProperty(nameof(PATToken), ValidateNotEmpty, CoerceEmptyStringToNull);
    }

    public ConnectionDto Clone() => (ConnectionDto)Clone(new ConnectionDto(Id));
}
