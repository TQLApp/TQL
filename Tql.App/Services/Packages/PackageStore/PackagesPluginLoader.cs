using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Services.Packages.AssemblyResolution;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages.PackageStore;

internal class PackagesPluginLoader : IPluginLoader
{
    public const string ManifestFileName = "tqlpackage.manifest.json";

    private const int ManifestVersion = 1;

    public static void WritePackageManifest(string targetPath, ILogger logger)
    {
        logger.LogInformation("Writing package manifest");

        var entries = PackageExportFinder.Resolve(targetPath, logger);

        if (entries.Count == 0)
            throw new InvalidOperationException("Could not discover assembly entry points");

        var manifest = new PackageManifest(ManifestVersion, entries.ToImmutableArray());

        var json = JsonSerializer.Serialize(manifest);

        File.WriteAllText(Path.Combine(targetPath, ManifestFileName), json);
    }

    private readonly PackageStoreManager _packageStoreManager;
    private readonly ILogger _logger;
    private readonly AssemblyResolver _assemblyResolver;

    public PackagesPluginLoader(PackageStoreManager packageStoreManager, ILogger logger)
    {
        _packageStoreManager = packageStoreManager;
        _logger = logger;

        _assemblyResolver = AssemblyResolver.Create(
            _packageStoreManager
                .GetInstalledPackages()
                .Select(p => Path.Combine(_packageStoreManager.PackagesFolder, p.ToString()))
                .Concat(new[] { Path.GetDirectoryName(GetType().Assembly.Location)! }),
            _logger
        );
    }

    public ImmutableArray<ITqlPlugin> GetPlugins()
    {
        _logger.LogInformation("Discovering plugins");

        var plugins = ImmutableArray.CreateBuilder<ITqlPlugin>();

        foreach (var packageRef in _packageStoreManager.GetInstalledPackages())
        {
            var packageFolder = Path.Combine(
                _packageStoreManager.PackagesFolder,
                packageRef.ToString()
            );

            _logger.LogInformation("Loading plugins from '{PackageFolder}'", packageFolder);

            try
            {
                var manifestJson = File.ReadAllText(Path.Combine(packageFolder, ManifestFileName));
                var manifest = JsonSerializer.Deserialize<PackageManifest>(manifestJson)!;

                foreach (var assemblyEntries in manifest.Entries.GroupBy(p => p.FileName))
                {
                    var assembly = Assembly.LoadFile(
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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin from '{Package}'", packageFolder);
            }
        }

        return plugins.ToImmutable();
    }

    public void Dispose()
    {
        _assemblyResolver.Dispose();
    }
}
