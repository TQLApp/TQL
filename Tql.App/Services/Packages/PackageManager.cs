using System.IO;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Tql.Abstractions;
using Tql.App.Services.Packages.NuGet;
using Tql.App.Services.Packages.PackageStore;
using Tql.App.Services.Telemetry;
using Tql.Utilities;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages;

internal class PackageManager
{
    private static readonly char[] SpaceSeparator = { ' ' };
    private const string TagName = "tql-plugin";

    private readonly HttpClient _httpClient;
    private readonly ILogger<PackageManager> _logger;
    private readonly PackageStoreManager _storeManager;
    private readonly IConfigurationManager _configurationManager;
    private readonly TelemetryService _telemetryService;
    private readonly IEncryption _encryption;
    private volatile PackageManagerConfiguration _configuration =
        PackageManagerConfiguration.Default;

    public PackageManagerConfiguration Configuration => _configuration;

    public PackageManager(
        HttpClient httpClient,
        ILogger<PackageManager> logger,
        PackageStoreManager storeManager,
        IConfigurationManager configurationManager,
        TelemetryService telemetryService,
        IEncryption encryption
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _storeManager = storeManager;
        _configurationManager = configurationManager;
        _telemetryService = telemetryService;
        _encryption = encryption;

        configurationManager.ConfigurationChanged += (_, e) =>
        {
            if (e.PluginId == Constants.PackageManagerConfigurationId)
                LoadConfiguration(e.Configuration);
        };

        LoadConfiguration(
            configurationManager.GetConfiguration(Constants.PackageManagerConfigurationId)
        );
    }

    private void LoadConfiguration(string? json)
    {
        var configuration = default(PackageManagerConfiguration);

        if (json != null)
        {
            try
            {
                configuration = JsonSerializer.Deserialize<PackageManagerConfiguration>(json);
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Failed to deserialize configuration");
            }
        }

        _configuration = configuration ?? PackageManagerConfiguration.Default;
    }

    public void UpdateConfiguration(PackageManagerConfiguration configuration)
    {
        var json = JsonSerializer.Serialize(configuration);

        _configurationManager.SetConfiguration(Constants.PackageManagerConfigurationId, json);
    }

    private NuGetClient GetClient()
    {
        var sources = ImmutableArray.CreateBuilder<NuGetClientSource>();

        foreach (var source in _configuration.Sources)
        {
            var credentials = default(PackageSourceCredential);
            if (source.UserName != null)
            {
                credentials = new PackageSourceCredential(
                    source.Url,
                    source.UserName,
                    _encryption.DecryptString(source.ProtectedPassword),
                    true,
                    null
                );
            }

            sources.Add(new NuGetClientSource(source.Url, credentials));
        }

        var packageCachePath = Path.Combine(Path.GetTempPath(), "TQL Package Source");

        return new NuGetClient(
            new NuGetClientConfiguration(packageCachePath, sources.ToImmutable()),
            new NuGetLogger(_logger),
            Constants.ApplicationFrameworkVersion,
            Constants.SystemPackageIds
        );
    }

    public async Task<ImmutableArray<Package>> GetAvailablePackages()
    {
        var packages = await GetUpstreamPackages();

        var installedPackages = _storeManager.GetInstalledPackages();

        var packageDefinitions = new List<Package>();

        foreach (var package in packages)
        {
            packageDefinitions.Add(
                new Package(
                    package.Identity,
                    package.Description,
                    package.DownloadCount.GetValueOrDefault(),
                    package.Authors,
                    await GetIcon(package.IconUrl),
                    installedPackages.Any(
                        p1 =>
                            string.Equals(
                                p1.Id,
                                package.Identity.Id,
                                StringComparison.OrdinalIgnoreCase
                            )
                    ),
                    IsVerified(package.Identity.Id)
                )
            );
        }

        return packageDefinitions.ToImmutableArray();
    }

    private bool IsVerified(string identityId)
    {
        return identityId.StartsWith("TQLApp.", StringComparison.OrdinalIgnoreCase);
    }

    public async Task InstallPackage(
        string packageId,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogInformation("Installing '{Id}'", packageId);

        if (_storeManager.GetInstalledVersion(packageId) != null)
        {
            _logger.LogWarning("Skipping install because the package is installed already");
            return;
        }

        using (var @event = _telemetryService.CreateEvent("Install Package"))
        {
            @event.AddProperty("Package Id", packageId);
        }

        await EnsureInstalled(packageId, cancellationToken);
    }

    private async Task<bool> EnsureInstalled(
        string packageId,
        CancellationToken cancellationToken = default
    )
    {
        using var client = GetClient();

        var packages = await client.GetPackageMetadata(packageId, false, false, cancellationToken);
        if (packages.Length == 0)
            throw new InvalidOperationException($"Cannot find package '{packageId}'");

        var latestVersion = packages.OrderByDescending(p => p.Identity.Version).First();

        var installedVersion = _storeManager.GetInstalledVersion(packageId);

        if (
            installedVersion != null
            && new Version(installedVersion).Equals(latestVersion.Identity.Version.Version)
        )
        {
            _logger.LogInformation("Package '{Package}' is already up to date", packageId);
            return false;
        }

        using (var @event = _telemetryService.CreateEvent("Update Package"))
        {
            @event.AddProperty("Package Id", packageId);
            @event.AddProperty(
                "Package Version",
                latestVersion.Identity.Version.Version.ToString()
            );
        }

        _logger.LogInformation(
            "Installing package '{Package}' version '{Version}'",
            latestVersion.Identity.Id,
            latestVersion.Identity.Version.Version
        );

        var targetPath = Path.Combine(
            _storeManager.PackagesFolder,
            PackageRef.FromIdentity(latestVersion.Identity).ToString()
        );

        if (Directory.Exists(targetPath))
        {
            _logger.LogInformation("Cleaning up old target path");

            var tempTargetPath = targetPath + "-DELETED";
            if (Directory.Exists(tempTargetPath))
                Directory.Delete(tempTargetPath, true);

            Directory.Move(targetPath, tempTargetPath);

            Directory.Delete(tempTargetPath, true);
        }

        Directory.CreateDirectory(targetPath);

        var abstractionsPackageIdentity = new PackageIdentity(
            "TQLApp.Abstractions",
            new NuGetVersion(typeof(ITqlPlugin).Assembly.GetName().Version!)
        );

        var installedPackages = await client.InstallPackage(
            latestVersion.Identity,
            requiredDependency: abstractionsPackageIdentity,
            cancellationToken: cancellationToken
        );

        foreach (var installedPackage in installedPackages)
        {
            foreach (var fileName in client.GetPackageFiles(installedPackage))
            {
                CopyDirectory(Path.GetDirectoryName(fileName)!, targetPath);
            }
        }

        _storeManager.SetPackageVersion(
            latestVersion.Identity.Id,
            latestVersion.Identity.Version.Version
        );

        return true;
    }

    private void CopyDirectory(string source, string target)
    {
        _logger.LogDebug("Copying '{Directory}'", source);

        foreach (var path in Directory.GetDirectories(source))
        {
            var targetPath = Path.Combine(target, Path.GetFileName(path));

            Directory.CreateDirectory(targetPath);

            CopyDirectory(path, targetPath);
        }

        foreach (var path in Directory.GetFiles(source))
        {
            _logger.LogDebug("Copying '{FileName}'", path);

            File.Copy(path, Path.Combine(target, Path.GetFileName(path)), true);
        }
    }

    public void RemovePackage(string packageId)
    {
        _logger.LogInformation("Removing '{Id}'", packageId);

        if (_storeManager.GetInstalledVersion(packageId) == null)
        {
            _logger.LogWarning("Skipping removal because the package is not installed");
            return;
        }

        using (var @event = _telemetryService.CreateEvent("Removing Package"))
        {
            @event.AddProperty("Package Id", packageId);
        }

        _storeManager.RemovePackage(packageId);
    }

    private async Task<ImageSource> GetIcon(Uri? iconUrl)
    {
        if (iconUrl == null)
            return Images.NuGet;

        try
        {
            using var stream = await _httpClient.GetStreamAsync(iconUrl);

            return ImageFactory.CreateBitmapImage(stream);
        }
        catch
        {
            return Images.NuGet;
        }
    }

    private async Task<List<IPackageSearchMetadata>> GetUpstreamPackages()
    {
        using var client = GetClient();

        var packages = await client.SearchPackages($"tags:{TagName}", "Tql", false, false, 1000);

        return packages.Where(p => HasTag(p, TagName)).ToList();
    }

    private bool HasTag(IPackageSearchMetadata package, string expectedTag)
    {
        if (package.Tags == null)
            return false;

        foreach (
            var tag in package.Tags.Split(SpaceSeparator, StringSplitOptions.RemoveEmptyEntries)
        )
        {
            if (string.Equals(tag, expectedTag, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    public async Task<bool> UpdatePlugins(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking for plugin updates");

        var anyUpdated = false;

        foreach (var installed in _storeManager.GetInstalledPackages())
        {
            if (await EnsureInstalled(installed.Id, cancellationToken))
                anyUpdated = true;
        }

        return anyUpdated;
    }
}
