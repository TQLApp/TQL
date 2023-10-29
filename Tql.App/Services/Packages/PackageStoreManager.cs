using System.IO;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Tql.Abstractions;
using Tql.App.Support;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages;

internal class PackageStoreManager
{
    private static readonly string[] AssemblyExtensions = { ".dll", ".exe" };

    private readonly Store _store;
    private readonly ILogger<PackageStoreManager> _logger;
    private readonly object _syncRoot = new();
    private readonly Dictionary<string, Assembly> _loadedAssemblies =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _packageAssemblies =
        new(StringComparer.OrdinalIgnoreCase);

    public string PackagesFolder { get; }

    public PackageStoreManager(Store store, ILogger<PackageStoreManager> logger)
    {
        _store = store;
        _logger = logger;

        PackagesFolder = Path.Combine(store.DataFolder, "Packages");

        Directory.CreateDirectory(PackagesFolder);

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        _logger.LogDebug("Attempting to resolve '{AssemblyName}'", args.Name);

        lock (_syncRoot)
        {
            var assemblyName = args.Name.Split(',').First().Trim();

            var loadedAssembly = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(p => (AssemblyName: p.GetName(), Assembly: p))
                .Where(
                    p =>
                        string.Equals(
                            p.AssemblyName.Name,
                            assemblyName,
                            StringComparison.OrdinalIgnoreCase
                        )
                )
                .OrderByDescending(p => p.AssemblyName.Version)
                .FirstOrDefault();

            if (loadedAssembly.Assembly != null)
            {
                _logger.LogDebug(
                    "Using already loaded assembly '{AssemblyName}'",
                    loadedAssembly.AssemblyName
                );

                return loadedAssembly.Assembly;
            }

            if (_packageAssemblies.TryGetValue(assemblyName, out var fileName))
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

        var assemblyExtensions = new HashSet<string>(
            AssemblyExtensions,
            StringComparer.OrdinalIgnoreCase
        );

        var pluginAssemblyFileNames = new List<string>();
        var packageAssemblies = new List<(AssemblyName AssemblyName, string FileName)>();

        foreach (var packageRef in GetInstalledPackages())
        {
            var packageFolder = Path.Combine(PackagesFolder, packageRef.ToString());

            _logger.LogInformation("Loading assemblies of package '{Package}'", packageRef);

            foreach (var fileName in Directory.GetFiles(packageFolder))
            {
                if (assemblyExtensions.Contains(Path.GetExtension(fileName)))
                {
                    try
                    {
                        var assemblyName = AssemblyName.GetAssemblyName(fileName);

                        packageAssemblies.Add((assemblyName, fileName));
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

            pluginAssemblyFileNames.Add(Path.Combine(packageFolder, packageRef.Id + ".dll"));
        }

        lock (_syncRoot)
        {
            foreach (
                var entry in packageAssemblies
                    .GroupBy(p => p.AssemblyName.Name)
                    .Select(
                        p =>
                            (
                                AssemblyName: p.Key,
                                FileName: p.OrderByDescending(p1 => p1.AssemblyName.Version)
                                    .Select(p1 => p1.FileName)
                                    .First()
                            )
                    )
            )
            {
                _packageAssemblies.Add(entry.AssemblyName, entry.FileName);
            }
        }

        var plugins = ImmutableArray.CreateBuilder<ITqlPlugin>();

        foreach (var fileName in pluginAssemblyFileNames)
        {
            _logger.LogInformation("Loading plugins from '{FileName}'", fileName);

            try
            {
                var assembly = Assembly.LoadFile(fileName);

                foreach (var type in assembly.ExportedTypes)
                {
                    var attribute = type.GetCustomAttribute<TqlPluginAttribute>();
                    if (attribute == null)
                        continue;

                    if (!typeof(ITqlPlugin).IsAssignableFrom(type))
                    {
                        throw new InvalidOperationException(
                            $"'{type}' does not implement '{nameof(ITqlPlugin)}'"
                        );
                    }

                    _logger.LogInformation("Discovered plugin type '{Type}'", type);

                    plugins.Add((ITqlPlugin)Activator.CreateInstance(type)!);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load plugin from '{Package}'", fileName);
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
}
