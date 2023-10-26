using System.Diagnostics;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using Tql.Abstractions;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages;

internal class PackageStoreManager
{
    private static readonly string[] AssemblyExtensions = { ".dll", ".exe" };

    private readonly Store _store;
    private readonly object _syncRoot = new();
    private readonly List<string> _packageFolders = new();

    public string PackagesFolder { get; }

    public PackageStoreManager(Store store)
    {
        _store = store;

        PackagesFolder = Path.Combine(store.DataFolder, "Packages");

        Directory.CreateDirectory(PackagesFolder);

        AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
    }

    private Assembly? CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
        var assemblyName = args.Name.Split(',').First().Trim();

        lock (_syncRoot)
        {
            foreach (var packageFolder in _packageFolders)
            {
                foreach (var extension in AssemblyExtensions)
                {
                    var fileName = Path.Combine(packageFolder, assemblyName + extension);
                    if (File.Exists(fileName))
                        return Assembly.LoadFile(fileName);
                }
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
            Directory.Delete(Path.Combine(PackagesFolder, package), true);
        }
    }

    public ImmutableArray<ITqlPlugin> GetPlugins()
    {
        var plugins = ImmutableArray.CreateBuilder<ITqlPlugin>();

        foreach (var packageRef in GetInstalledPackages())
        {
            try
            {
                var packageFolder = Path.Combine(PackagesFolder, packageRef.ToString());

                lock (_syncRoot)
                {
                    // We insert the latest package folder at the top
                    // because this prefers loading new assemblies from that folder.
                    _packageFolders.Insert(0, packageFolder);
                }

                var assemblyFileName = Path.Combine(packageFolder, packageRef.Id + ".dll");

                var assembly = Assembly.LoadFile(assemblyFileName);

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

                    plugins.Add((ITqlPlugin)Activator.CreateInstance(type)!);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError($"Failed to load plugin '{packageRef}'");
                Trace.TraceError(ex.Message);
                Trace.TraceError(ex.StackTrace);
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
