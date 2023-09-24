using Launcher.Abstractions;

namespace Launcher.App.Services;

internal class PluginManager : IPluginManager
{
    public ImmutableArray<PluginEntry> Plugins { get; }

    public PluginManager(ImmutableArray<PluginEntry> plugins)
    {
        Plugins = plugins;
    }
}
