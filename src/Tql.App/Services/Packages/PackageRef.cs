using NuGet.Packaging.Core;

namespace Tql.App.Services.Packages;

internal record PackageRef(string Id, string Version)
{
    public static PackageRef FromIdentity(PackageIdentity identity) =>
        new(identity.Id, identity.Version.Version.ToString());

    public override string ToString() => $"{Id}.{Version}";
}
