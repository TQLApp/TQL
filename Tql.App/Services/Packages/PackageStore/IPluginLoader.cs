using Tql.Abstractions;

namespace Tql.App.Services.Packages.PackageStore;

internal interface IPluginLoader : IDisposable
{
    ImmutableArray<ITqlPlugin> GetPlugins();
}
