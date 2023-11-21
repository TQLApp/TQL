using Tql.Abstractions;

namespace Tql.App.Services.Packages.PackageStore;

internal class InstancePluginLoader(ImmutableArray<ITqlPlugin> plugins) : IPluginLoader
{
    public ImmutableArray<ITqlPlugin> GetPlugins() => plugins;

    public void Dispose() { }
}
