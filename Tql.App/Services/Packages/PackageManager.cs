using Microsoft.Extensions.Logging;
using NuGet.Configuration;
using System.Net.Http;
using NuGet.Protocol.Core.Types;
using Tql.App.Support;
using Tql.Utilities;
using Path = System.IO.Path;
using System.IO;
using Tql.Abstractions;
using Tql.App.Services.Telemetry;

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

    public PackageManager(
        HttpClient httpClient,
        ILogger<PackageManager> logger,
        PackageStoreManager storeManager,
        IConfigurationManager configurationManager,
        TelemetryService telemetryService
    )
    {
        _httpClient = httpClient;
        _logger = logger;
        _storeManager = storeManager;
        _configurationManager = configurationManager;
        _telemetryService = telemetryService;
    }

    private NuGetClient GetClient()
    {
        var configuration = PackageManagerConfiguration.FromJson(
            _configurationManager.GetConfiguration(Constants.PackageManagerConfigurationId)
        );

        var sources = ImmutableArray.CreateBuilder<NuGetClientSource>();

        foreach (var source in configuration.Sources)
        {
            var credentials = default(PackageSourceCredential);
            if (source.UserName != null)
            {
                credentials = new PackageSourceCredential(
                    source.Url,
                    source.UserName,
                    Encryption.Unprotect(source.ProtectedPassword),
                    true,
                    null
                );
            }

            sources.Add(new NuGetClientSource(source.Url, credentials));
        }

        var packageCachePath = Path.Combine(Path.GetTempPath(), "TQL Package Source");

        return new NuGetClient(
            new NuGetClientConfiguration(packageCachePath, sources.ToImmutable()),
            new NuGetLogger(_logger)
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
                    )
                )
            );
        }

        return packageDefinitions.ToImmutableArray();
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
        if (packages == null)
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

        var installedPackages = await client.InstallPackage(
            latestVersion.Identity,
            cancellationToken: cancellationToken
        );

        foreach (var installedPackage in installedPackages)
        {
            foreach (
                var fileName in client.GetPackageFiles(
                    installedPackage,
                    Constants.ApplicationFrameworkVersion
                )
            )
            {
                _logger.LogDebug("Copying '{FileName}'", fileName);

                File.Copy(fileName, Path.Combine(targetPath, Path.GetFileName(fileName)), true);
            }
        }

        _storeManager.SetPackageVersion(
            latestVersion.Identity.Id,
            latestVersion.Identity.Version.Version
        );

        return true;
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

        var packages = await client.SearchPackages(
#if DEBUG
            "Tql",
#else
            $"tags:{TagName}",
#endif
            false, false, 1000);

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
