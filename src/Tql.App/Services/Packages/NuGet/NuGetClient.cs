using System.IO;
using System.Text.RegularExpressions;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Packaging.Signing;
using NuGet.ProjectManagement;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using Tql.App.Support;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages.NuGet;

using PackageSource = global::NuGet.Configuration.PackageSource;

// Heavily based on https://gist.github.com/cpyfferoen/74092a74b165e85aed5ca1d51973b9d2.

internal class NuGetClient : IDisposable
{
    private readonly ILogger _logger;
    private readonly NuGetFramework _targetFramework;
    private readonly ImmutableArray<string> _systemPackageIds;
    private readonly List<SourceRepository> _remoteSourceRepositories = new();
    private readonly NuGetPackageManager _packageManager;
    private readonly List<Lazy<INuGetResourceProvider>> _providers =
        new(Repository.Provider.GetCoreV3());

    private readonly SourceCacheContext _sourceCacheContext = new();
    private readonly ISettings _settings = NullSettings.Instance;
    private readonly string _directDownloadPath;
    private readonly MyFolderNuGetProject _nuGetProject;

    public NuGetClient(
        NuGetClientConfiguration configuration,
        ILogger logger,
        NuGetFramework targetFramework,
        ImmutableArray<string> systemPackageIds
    )
    {
        _logger = logger;
        _targetFramework = targetFramework;
        _systemPackageIds = systemPackageIds;

        _sourceCacheContext.NoCache = true;

        foreach (var source in configuration.Sources)
        {
            var packageSource = new PackageSource(source.Source)
            {
                Credentials = source.Credentials
            };

            var sourceRepository = new SourceRepository(packageSource, _providers);

            _remoteSourceRepositories.Add(sourceRepository);
        }

        var packageSourceProvider = new PackageSourceProvider(_settings);
        var sourceRepositoryProvider = new SourceRepositoryProvider(
            packageSourceProvider,
            _providers
        );

        var packagesFolderPath = Path.GetFullPath(
            Path.Combine(configuration.PackageCachePath, "Packages")
        );
        Directory.CreateDirectory(packagesFolderPath);

        _directDownloadPath = Path.GetFullPath(
            Path.Combine(configuration.PackageCachePath, "Download")
        );
        Directory.CreateDirectory(_directDownloadPath);

        _nuGetProject = new MyFolderNuGetProject(
            packagesFolderPath,
            new PackagePathResolver(packagesFolderPath),
            targetFramework
        );

        _packageManager = new NuGetPackageManager(
            sourceRepositoryProvider,
            _settings,
            packagesFolderPath
        )
        {
            PackagesFolderNuGetProject = _nuGetProject,
        };
    }

    public async Task<ImmutableArray<NuGetSearchResult>> SearchPackages(
        string nuGetOrgSearchTerm,
        string otherSearchTerm,
        bool includePrerelease,
        bool includeDelisted,
        int maxResults,
        CancellationToken cancellationToken = default
    )
    {
        var packages = ImmutableArray.CreateBuilder<NuGetSearchResult>();

        foreach (var remoteNuGetFeed in _remoteSourceRepositories)
        {
            var searchTerm = IsNuGetOrgFeed(remoteNuGetFeed) ? nuGetOrgSearchTerm : otherSearchTerm;

            var searchResource = await remoteNuGetFeed.GetResourceAsync<PackageSearchResource>(
                cancellationToken
            );
            if (searchResource != null)
            {
                var searchFilter = new SearchFilter(includePrerelease)
                {
                    OrderBy = SearchOrderBy.Id,
                    IncludeDelisted = includeDelisted
                };

                packages.AddRange(
                    from package in await searchResource.SearchAsync(
                        searchTerm,
                        searchFilter,
                        0,
                        maxResults,
                        _logger,
                        cancellationToken
                    )
                    select new NuGetSearchResult(package, remoteNuGetFeed.PackageSource)
                );
            }
            else
            {
                var listResource = await remoteNuGetFeed.GetResourceAsync<ListResource>(
                    cancellationToken
                );

                var searchResults = await listResource.ListAsync(
                    searchTerm,
                    includePrerelease,
                    false,
                    includeDelisted,
                    _logger,
                    cancellationToken
                );

                var enumerator = searchResults.GetEnumeratorAsync();

                while (await enumerator.MoveNextAsync())
                {
                    packages.Add(
                        new NuGetSearchResult(enumerator.Current, remoteNuGetFeed.PackageSource)
                    );
                }
            }
        }

        return packages.ToImmutable();
    }

    private bool IsNuGetOrgFeed(SourceRepository sourceRepository)
    {
        var host = sourceRepository.PackageSource.SourceUri.Host;

        return Regex.IsMatch(host, @"\bnuget.org$");
    }

    public async Task<ImmutableArray<IPackageSearchMetadata>> GetPackageMetadata(
        string packageId,
        bool includePrerelease,
        bool includeUnlisted,
        CancellationToken cancellationToken = default
    )
    {
        var packages = ImmutableArray.CreateBuilder<IPackageSearchMetadata>();

        foreach (var remoteNuGetFeed in _remoteSourceRepositories)
        {
            var metadataResource = await remoteNuGetFeed.GetResourceAsync<PackageMetadataResource>(
                cancellationToken
            );

            if (metadataResource == null)
                throw new InvalidOperationException();

            var metadata = await metadataResource.GetMetadataAsync(
                packageId,
                includePrerelease,
                includeUnlisted,
                _sourceCacheContext,
                _logger,
                cancellationToken
            );

            packages.AddRange(metadata);
        }

        return packages.ToImmutable();
    }

    public ImmutableArray<string> GetPackageFiles(PackageIdentity packageId)
    {
        _logger.LogDebug($"Resolving package files for '{packageId}'");

        var package = LocalFolderUtility.GetPackage(
            new Uri(
                _packageManager.PackagesFolderNuGetProject.GetInstalledPackageFilePath(packageId)
            ),
            _logger
        );
        if (package == null)
            throw new ArgumentException("Cannot find package");

        var packagePath = Path.GetDirectoryName(package.Path)!;

        using var packageReader = package.GetReader();

        return GetPackageFiles(packageReader, packagePath);
    }

    private ImmutableArray<string> GetPackageFiles(
        IPackageContentReader packageReader,
        string packagePath
    )
    {
        var matchedReferenceItem = MSBuildNuGetProjectSystemUtility.GetMostCompatibleGroup(
            _targetFramework,
            packageReader.GetReferenceItems()
        );

        if (matchedReferenceItem == null)
            return ImmutableArray<string>.Empty;

        return matchedReferenceItem
            .Items
            .Select(p => Path.Combine(packagePath, p.Replace('/', Path.DirectorySeparatorChar)))
            .ToImmutableArray();
    }

    public async Task<ImmutableArray<PackageIdentity>> InstallPackage(
        PackageIdentity identity,
        PackageIdentity? requiredDependency,
        IProgress progress
    )
    {
        var resolutionContext = new ResolutionContext(
            dependencyBehavior: DependencyBehavior.Lowest,
            includePrelease: true,
            includeUnlisted: true,
            versionConstraints: VersionConstraints.None
        );
        var projectContext = new BlankProjectContext(NullSettings.Instance, _logger);

        progress.SetProgress(Labels.NuGetClient_FindingDependencies, 0);

        var installActions = (
            await _packageManager.PreviewInstallPackageAsync(
                _packageManager.PackagesFolderNuGetProject,
                identity,
                resolutionContext,
                projectContext,
                _remoteSourceRepositories,
                Array.Empty<SourceRepository>(),
                progress.CancellationToken
            )
        ).ToList();

        if (requiredDependency != null)
            VerifyRequiredDependency(identity, requiredDependency, installActions);

        TrimSystemPackages(installActions);

        var logger = new LoggerAdapter(projectContext);

        var downloadContext = new PackageDownloadContext(
            _sourceCacheContext,
            _directDownloadPath,
            true
        )
        {
            ParentId = projectContext.OperationId,
            ClientPolicyContext = ClientPolicyContext.GetClientPolicy(_settings, logger)
        };

        progress.SetProgress(Labels.NuGetClient_Installing, 0.1);

        var tracker = new InstallationTracker(
            installActions.Select(p => p.PackageIdentity),
            progress.GetSubProgress(0.1, 1)
        );

        _nuGetProject.PackageInstalled += tracker.PackageInstalled;

        try
        {
            await _packageManager.ExecuteNuGetProjectActionsAsync(
                _packageManager.PackagesFolderNuGetProject,
                installActions,
                projectContext,
                downloadContext,
                progress.CancellationToken
            );
        }
        finally
        {
            _nuGetProject.PackageInstalled -= tracker.PackageInstalled;
        }

        return installActions.Select(action => action.PackageIdentity).ToImmutableArray();
    }

    private void TrimSystemPackages(List<NuGetProjectAction> installActions)
    {
        var trim = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        trim.UnionWith(_systemPackageIds);

        loop:
        while (installActions.Count > 0)
        {
            foreach (var action in installActions.Where(p => trim.Contains(p.PackageIdentity.Id)))
            {
                _logger.LogInformation($"Removing system package '{action.PackageIdentity}'");

                installActions.Remove(action);

                if (action.PackageIdentity is SourcePackageDependencyInfo dependencyInfo)
                    trim.UnionWith(dependencyInfo.Dependencies.Select(p => p.Id));

                goto loop;
            }

            break;
        }
    }

    private static void VerifyRequiredDependency(
        PackageIdentity identity,
        PackageIdentity requiredDependency,
        List<NuGetProjectAction> installActions
    )
    {
        var resolvedDependency = installActions
            .Select(p => p.PackageIdentity)
            .Where(
                p => string.Equals(p.Id, requiredDependency.Id, StringComparison.OrdinalIgnoreCase)
            )
            .MaxBy(p => p.Version.Version);

        if (resolvedDependency == null)
        {
            throw new NuGetClientException(
                $"NuGet package '{identity.Id}' does not list '{requiredDependency.Id}' as a dependency"
            );
        }

        var requiredVersion = requiredDependency.Version.Version;
        var resolvedVersion = resolvedDependency.Version.Version;

        // Local builds have 0.0 as the version. Skip the version check in this case.
        if (
            !requiredVersion.Equals(new Version(0, 0, 0, 0))
            && resolvedVersion.CompareTo(requiredVersion) > 0
        )
        {
            throw new NuGetClientException(
                $"NuGet package '{identity.Id}' requires version '{resolvedVersion}' of "
                    + $"'{requiredDependency.Id}' which is higher then the supported version '{requiredVersion}'"
            );
        }
    }

    public void Dispose()
    {
        _sourceCacheContext.Dispose();
    }

    private class InstallationTracker
    {
        private readonly IProgress _progress;
        private readonly HashSet<string> _ids;
        private readonly int _count;

        public InstallationTracker(IEnumerable<PackageIdentity> ids, IProgress progress)
        {
            _progress = progress;
            _ids = ids.Select(p => p.Id).ToHashSet(StringComparer.OrdinalIgnoreCase);
            _count = _ids.Count;
        }

        public void PackageInstalled(object? sender, PackageIdentityEventArgs e)
        {
            _ids.Remove(e.Id.Id);

            var offset = _count - _ids.Count;

            _progress.SetProgress(
                string.Format(Labels.NuGetClient_PackageInstalled, e.Id),
                (double)offset / _count
            );
        }
    }
}

internal record NuGetSearchResult(IPackageSearchMetadata Package, PackageSource Source);
