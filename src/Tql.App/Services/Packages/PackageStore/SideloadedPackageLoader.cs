using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Services.Packages.AssemblyResolution;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages.PackageStore;

internal class SideloadedPackageLoader : IPackageLoader
{
    private readonly AssemblyLoadContext _assemblyLoadContext;
    private readonly string _path;
    private readonly ILogger _logger;
    private readonly AssemblyResolver _assemblyResolver;

    public SideloadedPackageLoader(
        AssemblyLoadContext assemblyLoadContext,
        string path,
        ILogger logger
    )
    {
        _assemblyLoadContext = assemblyLoadContext;
        _path = path;
        _logger = logger;

        _assemblyResolver = AssemblyResolver.Create(
            new[] { path, Path.GetDirectoryName(GetType().Assembly.Location)! },
            assemblyLoadContext,
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
            var assembly = _assemblyLoadContext.LoadFromAssemblyName(assemblyName);

            foreach (var type in PluginLoaderUtils.GetPluginTypes(assembly))
            {
                plugins.Add(type);
            }
        }

        return plugins.ToImmutable();
    }

    public ImmutableArray<Package> GetPackages()
    {
        var packages = ImmutableArray.CreateBuilder<Package>();

        foreach (var group in GetPluginTypes().GroupBy(p => p.Assembly))
        {
            var assemblyName = group.Key.GetName();
            var packageRef = new PackageRef(assemblyName.Name!, assemblyName.Version!.ToString());

            try
            {
                packages.Add(
                    new Package(
                        packageRef,
                        group
                            .Select(p => (ITqlPlugin)Activator.CreateInstance(p)!)
                            .ToImmutableArray(),
                        null
                    )
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugins from '{AssemblyName}'", assemblyName);
                packages.Add(new Package(packageRef, null, ex));
            }
        }

        return packages.ToImmutable();
    }

    public void Dispose()
    {
        _assemblyResolver.Dispose();
    }
}
