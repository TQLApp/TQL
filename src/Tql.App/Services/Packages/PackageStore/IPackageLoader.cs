using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.Services.Packages.PackageStore;

internal interface IPackageLoader : IDisposable
{
    ImmutableArray<Package> GetPackages(IProgress progress);
}

internal record Package(
    PackageRef Id,
    ImmutableArray<ITqlPlugin>? Plugins,
    Exception? LoadException
);
