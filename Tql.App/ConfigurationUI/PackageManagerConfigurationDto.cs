using System.Collections.ObjectModel;
using Tql.Abstractions;
using Tql.App.Services.Packages;
using Tql.App.Support;
using Tql.Utilities;

namespace Tql.App.ConfigurationUI;

internal class PackageManagerConfigurationDto
{
    public ObservableCollection<PackageSourceDto> Sources { get; } = new();

    public static PackageManagerConfigurationDto FromConfiguration(
        PackageManagerConfiguration configuration,
        IEncryption encryption
    )
    {
        var result = new PackageManagerConfigurationDto();

        result
            .Sources
            .AddRange(
                configuration
                    .Sources
                    .Select(
                        p =>
                            new PackageSourceDto
                            {
                                Url = p.Url,
                                UserName = p.UserName,
                                Password = encryption.DecryptString(p.ProtectedPassword)
                            }
                    )
            );

        return result;
    }

    public PackageManagerConfiguration ToConfiguration(IEncryption encryption)
    {
        return new PackageManagerConfiguration(
            Sources
                .Select(
                    p => new PackageSource(p.Url!, p.UserName, encryption.EncryptString(p.Password))
                )
                .ToImmutableArray()
        );
    }
}

internal class PackageSourceDto : DtoBase
{
    public string? Url
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public string? UserName
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public string? Password
    {
        get => (string?)GetValue();
        set => SetValue(value);
    }

    public PackageSourceDto()
    {
        AddProperty(
            nameof(Url),
            p => ValidateNotEmpty(p) ?? ValidateUrl(p),
            CoerceEmptyStringToNull
        );
        AddProperty(nameof(UserName), null, CoerceEmptyStringToNull);
        AddProperty(nameof(Password), null, CoerceEmptyStringToNull);
    }

    public PackageSourceDto Clone() => (PackageSourceDto)Clone(new PackageSourceDto());
}
