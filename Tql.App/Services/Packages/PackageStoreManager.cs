using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages;

internal class PackageStoreManager
{
    private const string ManifestFileName = "tqlpackage.manifest.json";
    private const int ManifestVersion = 1;

    private readonly Store _store;
    private readonly ILogger<PackageStoreManager> _logger;
    private readonly object _syncRoot = new();
    private readonly Dictionary<AssemblyKey, string> _packageAssemblies = new();

    public string PackagesFolder { get; }

    public PackageStoreManager(Store store, ILogger<PackageStoreManager> logger)
    {
        _store = store;
        _logger = logger;

        PackagesFolder = _store.PackagesFolder;

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        _logger.LogDebug("Attempting to resolve '{AssemblyName}'", args.Name);

        lock (_syncRoot)
        {
            var assemblyKey = AssemblyKey.FromName(new AssemblyName(args.Name));

            var loadedAssembly = PackageStoreUtils.GetLoadedAssembly(assemblyKey);

            if (loadedAssembly != null)
            {
                _logger.LogDebug(
                    "Using already loaded assembly '{AssemblyName}'",
                    loadedAssembly.GetName()
                );

                return loadedAssembly;
            }

            if (_packageAssemblies.TryGetValue(assemblyKey, out var fileName))
            {
                _logger.LogDebug("Resolved assembly to '{FileName}'", fileName);

                return Assembly.LoadFile(fileName);
            }
        }

        return null;
    }

    private RegistryKey CreatePackagesKey()
    {
        using var key = _store.CreateBaseKey();

        return key.CreateSubKey("Packages")!;
    }

    private List<string> GetAvailablePackages()
    {
        return Directory.GetDirectories(PackagesFolder).Select(Path.GetFileName).ToList();
    }

    public ImmutableArray<PackageRef> GetInstalledPackages()
    {
        var packages = ImmutableArray.CreateBuilder<PackageRef>();

        using var key = CreatePackagesKey();

        foreach (var id in key.GetValueNames())
        {
            var version = (string)key.GetValue(id);

            packages.Add(new PackageRef(id, version));
        }

        return packages.ToImmutable();
    }

    public void PerformCleanup()
    {
        var actual = GetAvailablePackages();
        var expected = GetInstalledPackages()
            .Select(p => p.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var package in actual.Where(p => !expected.Contains(p)))
        {
            _logger.LogInformation(
                "Deleting unused package folder for package '{Package}'",
                package
            );

            Directory.Delete(Path.Combine(PackagesFolder, package), true);
        }
    }

    public ImmutableArray<ITqlPlugin> GetPlugins()
    {
        _logger.LogInformation("Discovering plugins");

        var packages = FindPackageAssemblies();

        InitializePackageAssemblies(packages);

        return CreatePluginInstances(packages);
    }

    private List<Package> FindPackageAssemblies()
    {
        var packages = new List<Package>();

        foreach (var packageRef in GetInstalledPackages())
        {
            var packageFolder = Path.Combine(PackagesFolder, packageRef.ToString());

            try
            {
                _logger.LogInformation("Loading assemblies of package '{Package}'", packageRef);

                var packageAssemblies = new List<PackageAssembly>();

                foreach (var fileName in PackageStoreUtils.GetAssemblyFileNames(packageFolder))
                {
                    {
                        try
                        {
                            var assemblyName = AssemblyName.GetAssemblyName(fileName);

                            packageAssemblies.Add(new PackageAssembly(assemblyName, fileName));
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex,
                                "Failed to get assembly name for '{FileName}'",
                                fileName
                            );
                        }
                    }
                }

                packages.Add(new Package(packageFolder, packageAssemblies));
            }
            catch (Exception ex)
            {
                _logger.LogCritical(
                    ex,
                    "Failed to log assemblies from plugin folder '{PackageFolder}'",
                    packageFolder
                );
            }
        }

        return packages;
    }

    private void InitializePackageAssemblies(List<Package> packages)
    {
        lock (_syncRoot)
        {
            foreach (
                var entry in packages
                    .SelectMany(p => p.Assemblies)
                    .GroupBy(p => new AssemblyKey(p.Name.Name, p.Name.CultureName))
                    .Select(
                        p =>
                            (
                                AssemblyKey: p.Key,
                                FileName: p.OrderByDescending(p1 => p1.Name.Version)
                                    .Select(p1 => p1.FileName)
                                    .First()
                            )
                    )
            )
            {
                _packageAssemblies.Add(entry.AssemblyKey, entry.FileName);
            }
        }
    }

    private ImmutableArray<ITqlPlugin> CreatePluginInstances(List<Package> packages)
    {
        var plugins = ImmutableArray.CreateBuilder<ITqlPlugin>();

        foreach (var packageFolder in packages.Select(p => p.Path))
        {
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

    public void SetPackageVersion(string packageId, Version version)
    {
        using var key = CreatePackagesKey();

        key.SetValue(packageId, version.ToString());
    }

    public void RemovePackage(string packageId)
    {
        using var key = CreatePackagesKey();

        key.DeleteValue(packageId, false);
    }

    public string? GetInstalledVersion(string packageId)
    {
        using var key = CreatePackagesKey();

        return key.GetValue(packageId) as string;
    }

    public void WritePackageManifest(string targetPath)
    {
        _logger.LogInformation("Writing package manifest");

        var entries = PackageEntryResolver.Resolve(targetPath, _logger);

        if (entries.Count == 0)
            throw new InvalidOperationException("Could not discover assembly entry points");

        var manifest = new PackageManifest(ManifestVersion, entries.ToImmutableArray());

        var json = JsonSerializer.Serialize(manifest);

        File.WriteAllText(Path.Combine(targetPath, ManifestFileName), json);
    }

    public IEnumerable<ITqlPlugin> GetSideloadedPlugins(string path)
    {
        var assemblyNames = new List<AssemblyName>();

        lock (_syncRoot)
        {
            foreach (var fileName in PackageStoreUtils.GetAssemblyFileNames(Path.GetFullPath(path)))
            {
                var assemblyName = AssemblyName.GetAssemblyName(fileName);
                assemblyNames.Add(assemblyName);

                var key = AssemblyKey.FromName(assemblyName);

                _packageAssemblies.Add(key, fileName);
            }
        }

        foreach (var assemblyName in assemblyNames)
        {
            var assembly = Assembly.Load(assemblyName);

            foreach (var type in PackageStoreUtils.GetPackageTypes(assembly))
            {
                yield return (ITqlPlugin)Activator.CreateInstance(type);
            }
        }
    }

    private record Package(string Path, List<PackageAssembly> Assemblies);

    private record PackageAssembly(AssemblyName Name, string FileName);

    private record PackageManifest(int Version, ImmutableArray<PackageEntry> Entries);
}

internal record struct AssemblyKey(string Name, string? CultureName)
{
    public static AssemblyKey FromName(AssemblyName name) => new(name.Name, name.CultureName);

    public readonly bool Equals(AssemblyKey other)
    {
        return string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase)
            && string.Equals(CultureName, other.CultureName, StringComparison.OrdinalIgnoreCase);
    }

    public override readonly int GetHashCode()
    {
        unchecked
        {
            return (StringComparer.OrdinalIgnoreCase.GetHashCode(Name) * 397)
                ^ (
                    CultureName != null
                        ? StringComparer.OrdinalIgnoreCase.GetHashCode(CultureName)
                        : 0
                );
        }
    }
}
