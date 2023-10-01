using Launcher.Abstractions;

namespace Launcher.App.Services;

internal record PluginManager(ImmutableArray<ILauncherPlugin> Plugins) : IPluginManager
{
    public void Initialize(IServiceProvider serviceProvider)
    {
        foreach (var plugin in Plugins)
        {
            plugin.Initialize(serviceProvider);
        }
    }
};
