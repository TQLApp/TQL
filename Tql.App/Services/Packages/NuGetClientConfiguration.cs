using NuGet.Configuration;

namespace Tql.App.Services.Packages;

public record NuGetClientConfiguration(
    string PackageCachePath,
    ImmutableArray<NuGetClientSource> Sources
);

public record NuGetClientSource(string Source, PackageSourceCredential? Credentials);
