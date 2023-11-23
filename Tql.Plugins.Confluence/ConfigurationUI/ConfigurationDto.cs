using System.Collections.ObjectModel;
using Tql.Abstractions;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.ConfigurationUI;

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
                                UserName = p.UserName,
                                Password = encryption.DecryptString(p.ProtectedPassword)
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
                        new Connection(
                            p.Id,
                            p.Name!,
                            p.Url!,
                            p.UserName,
                            encryption.EncryptString(p.Password)!
                        )
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
        get => (string?)GetValue(nameof(Name));
        set => SetValue(nameof(Name), value);
    }

    public string? Url
    {
        get => (string?)GetValue(nameof(Url));
        set => SetValue(nameof(Url), value);
    }

    public string? UserName
    {
        get => (string?)GetValue(nameof(UserName));
        set => SetValue(nameof(UserName), value);
    }

    public string? Password
    {
        get => (string?)GetValue(nameof(Password));
        set => SetValue(nameof(Password), value);
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
        AddProperty(nameof(UserName), ValidateNotEmpty, CoerceEmptyStringToNull);
        AddProperty(nameof(Password), ValidateNotEmpty, CoerceEmptyStringToNull);
    }

    public ConnectionDto Clone() => (ConnectionDto)base.Clone(new ConnectionDto(Id));
}
