using NuGet.Common;
using NuGet.Configuration;
using NuGet.Frameworks;
using NuGet.PackageManagement;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Resolver;
using System.IO;
using Tql.App.Support;
using Path = System.IO.Path;

namespace Tql.App.Services.Packages;

// Heavily based on https://gist.github.com/cpyfferoen/74092a74b165e85aed5ca1d51973b9d2.

internal class NuGetClient : IDisposable
{
    private readonly ILogger _logger;
    private readonly List<SourceRepository> _remoteSourceRepositories = new();
    private readonly NuGetPackageManager _packageManager;
    private readonly List<Lazy<INuGetResourceProvider>> _providers =
        new(Repository.Provider.GetCoreV3());

    public SourceCacheContext SourceCacheContext { get; } = new();

    public NuGetClient(NuGetClientConfiguration configuration, ILogger logger)
    {
        _logger = logger;

        foreach (var source in configuration.Sources)
        {
            var packageSource = new PackageSource(source.Source)
            {
                Credentials = source.Credentials
            };

            var sourceRepository = new SourceRepository(packageSource, _providers);

            _remoteSourceRepositories.Add(sourceRepository);
        }

        var packageSourceProvider = new PackageSourceProvider(NullSettings.Instance);
        var sourceRepositoryProvider = new SourceRepositoryProvider(
            packageSourceProvider,
            _providers
        );

        var packageCachePath = Path.GetFullPath(configuration.PackageCachePath);

        Directory.CreateDirectory(packageCachePath);

        _packageManager = new NuGetPackageManager(
            sourceRepositoryProvider,
            NullSettings.Instance,
            packageCachePath
        )
        {
            PackagesFolderNuGetProject = new FolderNuGetProject(packageCachePath),
        };
    }

    public async Task<ImmutableArray<IPackageSearchMetadata>> SearchPackages(
        string searchTerm,
        bool includePrerelease,
        bool includeDelisted,
        int maxResults,
        CancellationToken cancellationToken = default
    )
    {
        var packages = ImmutableArray.CreateBuilder<IPackageSearchMetadata>();

        foreach (var remoteNuGetFeed in _remoteSourceRepositories)
        {
            var searchResource = await remoteNuGetFeed.GetResourceAsync<PackageSearchResource>(
                cancellationToken
            );
            if (searchResource != null)
            {
                var searchFilter = new SearchFilter(true)
                {
                    OrderBy = SearchOrderBy.Id,
                    IncludeDelisted = includeDelisted
                };

                packages.AddRange(
                    await searchResource.SearchAsync(
                        searchTerm,
                        searchFilter,
                        0,
                        maxResults,
                        _logger,
                        cancellationToken
                    )
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

                packages.AddRange(await searchResults.ToListAsync());
            }
        }

        return packages.ToImmutable();
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
                SourceCacheContext,
                _logger,
                cancellationToken
            );

            packages.AddRange(metadata);
        }

        return packages.ToImmutable();
    }

    public ImmutableArray<string> GetPackageFiles(
        PackageIdentity packageId,
        NuGetFramework targetFramework
    )
    {
        var package = LocalFolderUtility.GetPackage(
            new Uri(
                _packageManager.PackagesFolderNuGetProject.GetInstalledPackageFilePath(packageId)
            ),
            _logger
        );
        if (package == null)
            throw new ArgumentException("Cannot find package");

        var directoryName = Path.GetDirectoryName(package.Path)!;

        using var packageReader = package.GetReader();

        var referenceItems = packageReader
            .GetReferenceItems()
            .Where(
                p =>
                    DefaultCompatibilityProvider.Instance.IsCompatible(
                        targetFramework,
                        p.TargetFramework
                    )
            )
            .ToList();

        if (referenceItems.Count == 0)
            throw new InvalidOperationException("Package does not support the specified framework");

        FrameworkSpecificGroup? matchedReferenceItem;

        if (referenceItems.Count == 1)
        {
            matchedReferenceItem = referenceItems.Single();
        }
        else
        {
            matchedReferenceItem = referenceItems
                .Where(p => p.TargetFramework.Framework == targetFramework.Framework)
                .OrderByDescending(p => p.TargetFramework.Version)
                .FirstOrDefault();

            if (matchedReferenceItem == null)
            {
                matchedReferenceItem = referenceItems
                    .Where(
                        p =>
                            p.TargetFramework.Framework
                            == FrameworkConstants.FrameworkIdentifiers.NetStandard
                    )
                    .OrderByDescending(p => p.TargetFramework.Version)
                    .FirstOrDefault();
            }
        }

        if (matchedReferenceItem == null)
            throw new InvalidOperationException("Cannot determine the correct framework version");

        return matchedReferenceItem.Items
            .Select(p => Path.Combine(directoryName, p.Replace('/', Path.DirectorySeparatorChar)))
            .ToImmutableArray();
    }

    public async Task<ImmutableArray<PackageIdentity>> InstallPackage(
        PackageIdentity identity,
        DependencyBehavior dependencyBehavior = DependencyBehavior.HighestPatch,
        bool includePrerelease = true,
        bool allowUnlisted = false,
        VersionConstraints versionConstraints =
            VersionConstraints.ExactMajor | VersionConstraints.ExactMinor,
        CancellationToken cancellationToken = default
    )
    {
        var resolutionContext = new ResolutionContext(
            dependencyBehavior,
            includePrerelease,
            allowUnlisted,
            versionConstraints
        );
        var projectContext = new BlankProjectContext(NullSettings.Instance, _logger);

        var installActions = (
            await _packageManager.PreviewInstallPackageAsync(
                _packageManager.PackagesFolderNuGetProject,
                identity,
                resolutionContext,
                projectContext,
                _remoteSourceRepositories,
                Array.Empty<SourceRepository>(),
                cancellationToken
            )
        ).ToList();

        await _packageManager.ExecuteNuGetProjectActionsAsync(
            _packageManager.PackagesFolderNuGetProject,
            installActions,
            projectContext,
            SourceCacheContext,
            cancellationToken
        );

        return installActions.Select(action => action.PackageIdentity).ToImmutableArray();
    }

    public void Dispose()
    {
        SourceCacheContext.Dispose();
    }
}
