using Tql.Abstractions;

namespace Tql.App.Services;

internal interface IPluginManager
{
    ImmutableArray<ILauncherPlugin> Plugins { get; }
}
