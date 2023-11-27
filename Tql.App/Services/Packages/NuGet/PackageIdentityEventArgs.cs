using NuGet.Packaging.Core;

namespace Tql.App.Services.Packages.NuGet;

internal class PackageIdentityEventArgs(PackageIdentity id) : EventArgs
{
    public PackageIdentity Id { get; } = id;
}
