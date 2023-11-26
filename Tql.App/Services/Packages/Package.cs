using NuGet.Packaging.Core;

namespace Tql.App.Services.Packages;

internal record Package(
    PackageIdentity Identity,
    string Title,
    string Description,
    long DownloadCount,
    string Authors,
    ImageSource Icon,
    bool IsInstalled,
    bool IsVerified
);
