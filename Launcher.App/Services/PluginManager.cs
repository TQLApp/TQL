using Launcher.Abstractions;

namespace Launcher.App.Services;

internal record PluginManager(ImmutableArray<ILauncherPlugin> Plugins) : IPluginManager;
