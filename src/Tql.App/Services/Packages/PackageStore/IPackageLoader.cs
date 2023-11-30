using Tql.Abstractions;

namespace Tql.App.Services.Packages.PackageStore;

internal interface IPackageLoader : IDisposable
{
    ImmutableArray<Package> GetPackages();
}

internal record Package(
    PackageRef Id,
    ImmutableArray<ITqlPlugin>? Plugins,
    Exception? LoadException
);
