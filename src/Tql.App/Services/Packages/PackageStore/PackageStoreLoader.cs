using System.IO;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Services.Packages.AssemblyResolution;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages.PackageStore;

internal class PackageStoreLoader : IPackageLoader
{
    public const string ManifestFileName = "tqlpackage.manifest.json";

    private const int ManifestVersion = 1;

    private readonly PackageStoreManager _packageStoreManager;
    private readonly AssemblyLoadContext _assemblyLoadContext;
    private readonly ILogger _logger;
    private readonly AssemblyResolver _assemblyResolver;

    public PackageStoreLoader(
        PackageStoreManager packageStoreManager,
        AssemblyLoadContext assemblyLoadContext,
        ILogger logger
    )
    {
        _packageStoreManager = packageStoreManager;
        _assemblyLoadContext = assemblyLoadContext;
        _logger = logger;

        _assemblyResolver = AssemblyResolver.Create(
            _packageStoreManager
                .GetInstalledPackages()
                .Select(p => Path.Combine(_packageStoreManager.PackagesFolder, p.ToString()))
                .Concat(new[] { Path.GetDirectoryName(GetType().Assembly.Location)! }),
            assemblyLoadContext,
            _logger
        );
    }

    public ImmutableArray<Package> GetPackages()
    {
        _logger.LogInformation("Discovering plugins");

        var packages = ImmutableArray.CreateBuilder<Package>();

        foreach (var packageRef in _packageStoreManager.GetInstalledPackages())
        {
            var packageFolder = Path.Combine(
                Path.GetFullPath(_packageStoreManager.PackagesFolder),
                packageRef.ToString()
            );

            _logger.LogInformation("Loading plugins from '{PackageFolder}'", packageFolder);

            try
            {
                var manifestFileName = Path.Combine(packageFolder, ManifestFileName);

                if (!File.Exists(manifestFileName))
                    WritePackageManifest(packageFolder);

                var manifestJson = File.ReadAllText(manifestFileName);
                var manifest = JsonSerializer.Deserialize<PackageManifest>(manifestJson)!;

                var plugins = ImmutableArray.CreateBuilder<ITqlPlugin>();

                foreach (var assemblyEntries in manifest.Entries.GroupBy(p => p.FileName))
                {
                    var assembly = _assemblyLoadContext.LoadFromAssemblyPath(
                        Path.Combine(packageFolder, assemblyEntries.Key)
                    );

                    foreach (var entry in assemblyEntries)
                    {
                        var type = assembly.GetType(entry.TypeName);
                        if (type == null)
                        {
                            throw new InvalidOperationException(
                                $"Cannot find type '{entry.TypeName}'"
                            );
                        }

                        plugins.Add((ITqlPlugin)Activator.CreateInstance(type)!);
                    }
                }

                packages.Add(new Package(packageRef, plugins.ToImmutable(), null));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin from '{Package}'", packageFolder);

                packages.Add(new Package(packageRef, null, ex));
            }
        }

        return packages.ToImmutable();
    }

    private void WritePackageManifest(string targetPath)
    {
        this._logger.LogInformation("Writing package manifest");

        var entries = GetPackageExports(targetPath, _logger);

        if (entries.Count == 0)
            throw new InvalidOperationException("Could not discover assembly entry points");

        var manifest = new PackageManifest(ManifestVersion, entries.ToImmutableArray());

        var json = JsonSerializer.Serialize(manifest);

        File.WriteAllText(Path.Combine(targetPath, ManifestFileName), json);
    }

    private List<PackageExport> GetPackageExports(string path, ILogger logger)
    {
        using var loader = new SideloadedPackageLoader(AssemblyLoadContext.Default, path, logger);

        var entries = new List<(string FileName, string TypeName)>();

        foreach (var type in loader.GetPluginTypes())
        {
            logger.Log(LogLevel.Information, "Discovered {TypeName}", type.FullName);

            entries.Add((type.Assembly.Location, type.FullName!));
        }

        return entries.Select(p => new PackageExport(p.FileName, p.TypeName)).ToList();
    }

    public void Dispose()
    {
        _assemblyResolver.Dispose();
    }
}
