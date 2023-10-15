using Tql.Abstractions;

namespace Tql.App.Services;

internal interface IPluginManager
{
    ImmutableArray<ITqlPlugin> Plugins { get; }
}
