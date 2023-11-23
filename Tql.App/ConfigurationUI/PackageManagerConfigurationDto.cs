using System.Collections.ObjectModel;
using Tql.Abstractions;
using Tql.App.Services.Packages;
using Tql.App.Support;
using Tql.Utilities;

namespace Tql.App.ConfigurationUI;

internal class PackageManagerConfigurationDto
{
    public ObservableCollection<PackageManagerSourceDto> Sources { get; } = new();

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
                            new PackageManagerSourceDto
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
                    p =>
                        new PackageManagerSource(
                            p.Url!,
                            p.UserName,
                            encryption.EncryptString(p.Password)
                        )
                )
                .ToImmutableArray()
        );
    }
}

internal class PackageManagerSourceDto : DtoBase
{
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

    public PackageManagerSourceDto()
    {
        AddProperty(
            nameof(Url),
            p => ValidateNotEmpty(p) ?? ValidateUrl(p),
            CoerceEmptyStringToNull
        );
        AddProperty(nameof(UserName), null, CoerceEmptyStringToNull);
        AddProperty(nameof(Password), null, CoerceEmptyStringToNull);
    }

    public PackageManagerSourceDto Clone() =>
        (PackageManagerSourceDto)Clone(new PackageManagerSourceDto());
}
