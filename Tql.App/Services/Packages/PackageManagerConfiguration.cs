namespace Tql.App.Services.Packages;

internal record PackageManagerConfiguration(ImmutableArray<PackageManagerSource> Sources)
{
    public static readonly PackageManagerConfiguration Default = CreateDefault();

    private static PackageManagerConfiguration CreateDefault()
    {
        return new PackageManagerConfiguration(
            Constants.PackageSources
                .Select(p => new PackageManagerSource(p, null, null))
                .ToImmutableArray()
        );
    }
}

internal record PackageManagerSource(string Url, string? UserName, string? ProtectedPassword);
