using Launcher.Abstractions;

namespace Launcher.App.Services;

internal interface IPluginManager
{
    ImmutableArray<PluginEntry> Plugins { get; }
}

internal record struct PluginEntry(ILauncherPlugin Plugin, ImmutableArray<ICategory> Categories);
