using NuGet.Configuration;

namespace Tql.App.Services.Packages.NuGet;

public record NuGetClientConfiguration(
    string PackageCachePath,
    ImmutableArray<NuGetClientSource> Sources
);

public record NuGetClientSource(string Source, PackageSourceCredential? Credentials);
