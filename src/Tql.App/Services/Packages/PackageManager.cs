using System.IO;
using System.IO.Compression;
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
using Tql.App.Support;
using Tql.Utilities;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages;

internal class PackageManager : IDisposable
{
    private static readonly char[] SpaceSeparator = { ' ' };
    private const string TagName = "tql-plugin";
    private const string PackageCacheFolderName = "TQL Package Source";

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

        CleanupPackageSource();

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

        var packageCachePath = Path.Combine(Path.GetTempPath(), PackageCacheFolderName);

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

        foreach (var (package, source) in packages)
        {
            var isLocal = source.IsLocal;
            var isVerified = !isLocal && IsVerified(package.Identity.Id);

            packageDefinitions.Add(
                new Package(
                    package.Identity,
                    package.Title,
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
                    isVerified,
                    isLocal
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
        IProgress progress,
        PackageProgressMode progressMode
    )
    {
        progress.CanCancel = true;

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

        await EnsureInstalled(packageId, progress, progressMode);
    }

    private async Task<bool> EnsureInstalled(
        string packageId,
        IProgress progress,
        PackageProgressMode progressMode
    )
    {
        using var client = GetClient();

        if (progressMode == PackageProgressMode.Install)
            progress.SetProgress(Labels.PackageManager_GettingPackageMetadata, 0);

        var packages = await client.GetPackageMetadata(
            packageId,
            false,
            false,
            progress.CancellationToken
        );
        if (packages.Length == 0)
            throw new InvalidOperationException($"Cannot find package '{packageId}'");

        var latestVersion = packages.OrderByDescending(p => p.Identity.Version).First();

        var installedVersion = _storeManager.GetInstalledVersion(packageId);

        var isLatestInstalled =
            installedVersion != null
            && new Version(installedVersion).Equals(latestVersion.Identity.Version.Version);

        var isPresent =
            installedVersion != null
            && _storeManager
                .GetAvailablePackages()
                .Contains(
                    new PackageRef(packageId, installedVersion).ToString(),
                    StringComparer.OrdinalIgnoreCase
                );

        if (isPresent && isLatestInstalled)
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

        if (progressMode == PackageProgressMode.Update)
        {
            progress.SetProgress(
                isLatestInstalled
                    ? string.Format(Labels.PackageManager_RestoringPackage, latestVersion.Title)
                    : string.Format(Labels.PackageManager_UpdatingPackage, latestVersion.Title),
                0
            );
        }

        var installedPackages = await client.InstallPackage(
            latestVersion.Identity,
            requiredDependency: abstractionsPackageIdentity,
            progressMode == PackageProgressMode.Install
                ? progress.GetSubProgress(0.1, 0.9)
                : NullProgress.FromCancellationToken(progress.CancellationToken)
        );

        if (progressMode == PackageProgressMode.Install)
            progress.SetProgress(Labels.PackageManager_DeployingPackage, 0.9);

        foreach (var installedPackage in installedPackages)
        {
            foreach (var fileName in client.GetPackageFiles(installedPackage))
            {
                progress.CancellationToken.ThrowIfCancellationRequested();

                CopyDirectory(Path.GetDirectoryName(fileName)!, targetPath);
            }
        }

        progress.CancellationToken.ThrowIfCancellationRequested();

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
            if (string.Equals(iconUrl.Scheme, "file", StringComparison.OrdinalIgnoreCase))
                return await GetIconFromFile(iconUrl);

            return await GetIconFromHttp(iconUrl);
        }
        catch
        {
            return Images.NuGet;
        }
    }

    private async Task<ImageSource> GetIconFromFile(Uri iconUrl)
    {
        if (!iconUrl.Fragment.StartsWith("#"))
        {
            throw new ArgumentException(
                "Icon URL must have a fragment starting with a '#'",
                nameof(iconUrl)
            );
        }

        await using var stream = File.OpenRead(iconUrl.LocalPath);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read);

        var entryName = iconUrl.Fragment.Substring(1);
        var entry = archive.GetEntry(entryName);
        if (entry == null)
        {
            throw new ArgumentException(
                $"Cannot find entry '{entryName}' in package",
                nameof(iconUrl)
            );
        }

        await using var entryStream = entry.Open();

        return ImageFactory.CreateBitmapImage(entryStream);
    }

    private async Task<ImageSource> GetIconFromHttp(Uri iconUrl)
    {
        using var stream = await _httpClient.GetStreamAsync(iconUrl);

        return ImageFactory.CreateBitmapImage(stream);
    }

    private async Task<List<NuGetSearchResult>> GetUpstreamPackages()
    {
        using var client = GetClient();

        var packages = await client.SearchPackages($"tags:{TagName}", "Tql", false, false, 1000);

        return packages.Where(p => HasTag(p.Package, TagName)).ToList();
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

    public async Task<bool> UpdatePlugins(IProgress progress, PackageProgressMode progressMode)
    {
        _logger.LogInformation("Checking for plugin updates");

        var anyUpdated = false;

        foreach (var installed in _storeManager.GetInstalledPackages())
        {
            if (await EnsureInstalled(installed.Id, progress, progressMode))
                anyUpdated = true;
        }

        return anyUpdated;
    }

    private void CleanupPackageSource()
    {
        try
        {
            var packageCachePath = Path.Combine(Path.GetTempPath(), PackageCacheFolderName);

            if (!Directory.Exists(packageCachePath))
                return;

            var altPackageCachePath = packageCachePath + "~";

            if (Directory.Exists(altPackageCachePath))
                Directory.Delete(altPackageCachePath, true);

            Directory.Move(packageCachePath, altPackageCachePath);

            Directory.Delete(altPackageCachePath, true);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete package cache folder");
        }
    }

    public void Dispose()
    {
        CleanupPackageSource();
    }
}
