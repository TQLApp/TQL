using System.IO;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Tql.App.Services.Packages;

namespace Tql.App.Test;

[TestFixture]
internal class NuGetClientFixture
{
    [Test]
    public async Task ListPackages()
    {
        using var client = CreateClient();

        var packages = await client.SearchPackages("Tql", false, false, 1000);

        Assert.IsTrue(packages.Any(p => p.Identity.Id == "Tql.Plugins.Demo"));
    }

    [Test]
    public async Task InstallPackages()
    {
        using var client = CreateClient();

        var packageIdentity = new PackageIdentity("Tql.Plugins.Demo", new NuGetVersion("0.1"));

        var installedPackageIdentities = await client.InstallPackage(packageIdentity);

        var files = client.GetPackageFiles(packageIdentity, Constants.ApplicationFrameworkVersion);

        Assert.AreEqual(1, files.Length);
        Assert.AreEqual("Tql.Plugins.Demo.dll", Path.GetFileName(files[0]));
        Assert.IsTrue(File.Exists(files[0]));

        foreach (var installedPackageIdentity in installedPackageIdentities)
        {
            foreach (
                var file in client.GetPackageFiles(
                    installedPackageIdentity,
                    Constants.ApplicationFrameworkVersion
                )
            )
            {
                Console.WriteLine(file);
            }
        }
    }

    [Test]
    public async Task GetPackageById()
    {
        using var client = CreateClient();

        var packages = await client.GetPackageMetadata("Tql.Plugins.Demo", false, false);

        Assert.IsTrue(packages.All(p => p.Identity.Id == "Tql.Plugins.Demo"));
        Assert.IsTrue(packages.Any(p => p.Identity.Version.Equals(new SemanticVersion(0, 1, 0))));
        Assert.IsTrue(packages.Any(p => p.Identity.Version.Equals(new SemanticVersion(0, 1, 1))));
    }

    private NuGetClient CreateClient()
    {
        const string packageCachePath = "Package Cache";
        if (Directory.Exists(packageCachePath))
            Directory.Delete(packageCachePath, true);

        var configuration = new NuGetClientConfiguration(
            packageCachePath,
            ImmutableArray.Create(
                new NuGetClientSource(
                    "https://pvginkel.pkgs.visualstudio.com/Launcher/_packaging/TQLPlugins/nuget/v3/index.json",
                    new PackageSourceCredential(
                        Constants.PackageSource,
                        Environment.GetEnvironmentVariable("AzureDevOpsArtifactsUserName"),
                        Environment.GetEnvironmentVariable("AzureDevOpsArtifactsPatToken"),
                        true,
                        null
                    )
                ),
                new NuGetClientSource("https://api.nuget.org/v3/index.json", null)
            )
        );

#if false
        // Clear the HTTP cache.

        var httpCacheDirectory = NuGetEnvironment.GetFolderPath(NuGetFolderPath.HttpCacheDirectory);

        Directory.Delete(httpCacheDirectory, true);
#endif

        return new NuGetClient(configuration, new NuGetLogger());
    }
}
