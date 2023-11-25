namespace Tql.App.Services.Packages;

internal record PackageManagerConfiguration(ImmutableArray<PackageSource> Sources)
{
    public static readonly PackageManagerConfiguration Default = CreateDefault();

    private static PackageManagerConfiguration CreateDefault()
    {
        return new PackageManagerConfiguration(
            Constants
                .PackageSources
                .Select(p => new PackageSource(p, null, null))
                .ToImmutableArray()
        );
    }
}

internal record PackageSource(string Url, string? UserName, string? ProtectedPassword);
