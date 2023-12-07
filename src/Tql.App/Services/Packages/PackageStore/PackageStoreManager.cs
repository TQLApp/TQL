using System.IO;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Tql.App.Support;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages.PackageStore;

internal class PackageStoreManager
{
    private readonly Store _store;
    private readonly ILogger<PackageStoreManager> _logger;

    public string PackagesFolder { get; }

    public PackageStoreManager(Store store, ILogger<PackageStoreManager> logger)
    {
        _store = store;
        _logger = logger;

        PackagesFolder = _store.PackagesFolder;
    }

    private RegistryKey CreatePackagesKey()
    {
        using var key = _store.CreateBaseKey();

        return key.CreateSubKey("Packages")!;
    }

    public ImmutableArray<PackageRef> GetInstalledPackages()
    {
        var packages = ImmutableArray.CreateBuilder<PackageRef>();

        using var key = CreatePackagesKey();

        foreach (var id in key.GetValueNames())
        {
            if (key.GetValue(id) is string version)
                packages.Add(new PackageRef(id, version));
        }

        return packages.ToImmutable();
    }

    public void PerformCleanup(IProgress progress)
    {
        var actual = GetAvailablePackages();
        var expected = GetInstalledPackages()
            .Select(p => p.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var package in actual.Where(p => !expected.Contains(p)))
        {
            progress.SetProgress(Labels.PackageStoreManager_DeletingUnusedFiles, 0);

            _logger.LogInformation(
                "Deleting unused package folder for package '{Package}'",
                package
            );

            try
            {
                Directory.Delete(Path.Combine(PackagesFolder, package), true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete unused package");
            }
        }
    }

    public ImmutableArray<string> GetAvailablePackages()
    {
        return Directory
            .GetDirectories(PackagesFolder)
            .Select(p => Path.GetFileName(p)!)
            .ToImmutableArray();
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
