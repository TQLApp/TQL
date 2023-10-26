using System.Collections.ObjectModel;
using Tql.App.Services.Packages;
using Tql.App.Support;

namespace Tql.App.ConfigurationUI;

internal class PackageManagerConfigurationDto
{
    public ObservableCollection<PackageManagerSourceDto> Sources { get; } = new();

    public static PackageManagerConfigurationDto FromConfiguration(
        PackageManagerConfiguration configuration
    )
    {
        var result = new PackageManagerConfigurationDto();

        result.Sources.AddRange(
            configuration.Sources.Select(
                p =>
                    new PackageManagerSourceDto
                    {
                        Url = p.Url,
                        UserName = p.UserName,
                        ProtectedPassword = p.ProtectedPassword
                    }
            )
        );

        return result;
    }

    public PackageManagerConfiguration ToConfiguration()
    {
        return new PackageManagerConfiguration(
            Sources
                .Select(p => new PackageManagerSource(p.Url!, p.UserName, p.ProtectedPassword))
                .ToImmutableArray()
        );
    }
}

internal class PackageManagerSourceDto
{
    public string? Url { get; set; }
    public string? UserName { get; set; }
    public byte[]? ProtectedPassword { get; set; }

    public string? Password
    {
        get => Encryption.Unprotect(ProtectedPassword);
        set => ProtectedPassword = Encryption.Protect(value);
    }

    public bool GetIsValid()
    {
        return !string.IsNullOrWhiteSpace(Url);
    }
}
