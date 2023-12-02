using System.Reflection;
using Tql.Abstractions;
using Tql.App.Support;

namespace Tql.App.Services.Packages.PackageStore;

internal class AssemblyPackageLoader(ImmutableArray<Assembly> assemblies) : IPackageLoader
{
    public ImmutableArray<Package> GetPackages(IProgress progress)
    {
        // We're not splitting these up into separate packages. This
        // package loader is only for the debug app, so it's less
        // important that exceptions are bubbled up here (hence the
        // lack of a try/catch).

        var plugins = (
            from assembly in assemblies
            from type in PluginLoaderUtils.GetPluginTypes(assembly)
            select (ITqlPlugin)Activator.CreateInstance(type)!
        ).ToImmutableArray();

        return ImmutableArray.Create(
            new Package(new PackageRef("DebugApp", "0.0"), plugins.ToImmutableArray(), null)
        );
    }

    public void Dispose() { }
}
