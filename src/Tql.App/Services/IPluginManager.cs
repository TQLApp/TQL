using Tql.Abstractions;

namespace Tql.App.Services;

internal interface IPluginManager
{
    ImmutableArray<IAvailablePackage> Packages { get; }
    ImmutableArray<ITqlPlugin> Plugins { get; }
}
