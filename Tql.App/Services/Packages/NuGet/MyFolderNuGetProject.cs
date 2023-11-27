using NuGet.Frameworks;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.ProjectManagement;
using NuGet.Protocol.Core.Types;

namespace Tql.App.Services.Packages.NuGet;

internal class MyFolderNuGetProject : FolderNuGetProject
{
    public event EventHandler<PackageIdentityEventArgs>? PackageInstalled;
    public event EventHandler<PackageIdentityEventArgs>? PackageUninstalled;

    public MyFolderNuGetProject(string root)
        : base(root) { }

    public MyFolderNuGetProject(string root, PackagePathResolver packagePathResolver)
        : base(root, packagePathResolver) { }

    public MyFolderNuGetProject(
        string root,
        PackagePathResolver packagePathResolver,
        NuGetFramework targetFramework
    )
        : base(root, packagePathResolver, targetFramework) { }

    public override async Task<bool> InstallPackageAsync(
        PackageIdentity packageIdentity,
        DownloadResourceResult downloadResourceResult,
        INuGetProjectContext nuGetProjectContext,
        CancellationToken token
    )
    {
        var result = await base.InstallPackageAsync(
            packageIdentity,
            downloadResourceResult,
            nuGetProjectContext,
            token
        );

        OnPackageInstalled(new PackageIdentityEventArgs(packageIdentity));

        return result;
    }

    public override async Task<bool> UninstallPackageAsync(
        PackageIdentity packageIdentity,
        INuGetProjectContext nuGetProjectContext,
        CancellationToken token
    )
    {
        var result = await base.UninstallPackageAsync(packageIdentity, nuGetProjectContext, token);

        OnPackageUninstalled(new PackageIdentityEventArgs(packageIdentity));

        return result;
    }

    protected virtual void OnPackageInstalled(PackageIdentityEventArgs e) =>
        PackageInstalled?.Invoke(this, e);

    protected virtual void OnPackageUninstalled(PackageIdentityEventArgs e) =>
        PackageUninstalled?.Invoke(this, e);
}
