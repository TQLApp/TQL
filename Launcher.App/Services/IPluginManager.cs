using Launcher.Abstractions;

namespace Launcher.App.Services;

internal interface IPluginManager
{
    ImmutableArray<ILauncherPlugin> Plugins { get; }
}
