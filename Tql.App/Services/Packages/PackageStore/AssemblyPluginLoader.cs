using System.Reflection;
using Tql.Abstractions;

namespace Tql.App.Services.Packages.PackageStore;

internal class AssemblyPluginLoader(ImmutableArray<Assembly> assemblies) : IPluginLoader
{
    public ImmutableArray<ITqlPlugin> GetPlugins()
    {
        return (
            from assembly in assemblies
            from type in PluginLoaderUtils.GetPluginTypes(assembly)
            select (ITqlPlugin)Activator.CreateInstance(type)!
        ).ToImmutableArray();
    }

    public void Dispose() { }
}
