using System.Reflection;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Services.Packages.AssemblyResolution;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages.PackageStore;

internal class SideloadedPluginLoader : IPluginLoader
{
    private readonly string _path;
    private readonly ILogger _logger;
    private readonly AssemblyResolver _assemblyResolver;

    public SideloadedPluginLoader(string path, ILogger logger)
    {
        _path = path;
        _logger = logger;

        _assemblyResolver = AssemblyResolver.Create(
            new[] { path, Path.GetDirectoryName(GetType().Assembly.Location)! },
            _logger
        );
    }

    public ImmutableArray<Type> GetPluginTypes()
    {
        _logger.LogInformation("Discovering sideloaded plugins");

        var plugins = ImmutableArray.CreateBuilder<Type>();

        foreach (var fileName in AssemblyUtils.GetAssemblyFileNames(Path.GetFullPath(_path)))
        {
            var assemblyName = AssemblyName.GetAssemblyName(fileName);
            var assembly = Assembly.Load(assemblyName);

            foreach (var type in PluginLoaderUtils.GetPluginTypes(assembly))
            {
                plugins.Add(type);
            }
        }

        return plugins.ToImmutable();
    }

    public ImmutableArray<ITqlPlugin> GetPlugins()
    {
        return GetPluginTypes()
            .Select(p => (ITqlPlugin)Activator.CreateInstance(p)!)
            .ToImmutableArray();
    }

    public void Dispose()
    {
        _assemblyResolver.Dispose();
    }
}
