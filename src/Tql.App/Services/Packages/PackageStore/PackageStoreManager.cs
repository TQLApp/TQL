using System.IO;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.App.Support;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages.PackageStore;

internal partial class PackageStoreManager
{
    private readonly Store _store;
    private readonly IConfigurationManager _configurationManager;
    private readonly ILogger<PackageStoreManager> _logger;

    private ConfigurationDto Configuration
    {
        get
        {
            var json = _configurationManager.GetConfiguration(
                Constants.PackageStoreConfigurationId
            );
            if (json == null)
                return new ConfigurationDto(ImmutableArray<PackageRef>.Empty);

            return JsonSerializer.Deserialize<ConfigurationDto>(json)!;
        }
        set
        {
            _configurationManager.SetConfiguration(
                Constants.PackageStoreConfigurationId,
                JsonSerializer.Serialize(value)
            );
        }
    }

    public string PackagesFolder { get; }

    public PackageStoreManager(
        Store store,
        IConfigurationManager configurationManager,
        ILogger<PackageStoreManager> logger
    )
    {
        _store = store;
        _configurationManager = configurationManager;
        _logger = logger;

        PackagesFolder = _store.PackagesFolder;

        MigrateConfiguration();
    }

    public ImmutableArray<PackageRef> GetInstalledPackages() => Configuration.Packages;

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
        var packages = Configuration
            .Packages
            .RemoveAll(p => string.Equals(p.Id, packageId, StringComparison.OrdinalIgnoreCase))
            .Add(new PackageRef(packageId, version.ToString()));

        Configuration = new ConfigurationDto(packages);
    }

    public void RemovePackage(string packageId)
    {
        var packages = Configuration
            .Packages
            .RemoveAll(p => string.Equals(p.Id, packageId, StringComparison.OrdinalIgnoreCase));

        Configuration = new ConfigurationDto(packages);
    }

    public string? GetInstalledVersion(string packageId)
    {
        return Configuration
            .Packages
            .SingleOrDefault(
                p => string.Equals(p.Id, packageId, StringComparison.OrdinalIgnoreCase)
            )
            ?.Version;
    }

    private record ConfigurationDto(ImmutableArray<PackageRef> Packages);
}
