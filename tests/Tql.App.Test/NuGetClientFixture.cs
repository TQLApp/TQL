using System.IO;
using NuGet.Packaging.Core;
using NuGet.Versioning;
using Tql.App.Services.Packages.NuGet;
using Tql.App.Support;

namespace Tql.App.Test;

[TestFixture]
internal class NuGetClientFixture
{
    [Test]
    public async Task ListPackages()
    {
        using var client = CreateClient();

        var packages = await client.SearchPackages("tags:tql-plugin", "Tql", false, false, 1000);

        Assert.IsTrue(packages.Any(p => p.Package.Identity.Id == "Tql.Plugins.Demo"));
    }

    [TestCase("TQLApp.Plugins.Azure", "Tql.Plugins.Azure.dll", "0.9.1")]
    [TestCase("TQLApp.Plugins.AzureDevOps", "Tql.Plugins.AzureDevOps.dll", "0.9.1")]
    [TestCase("TQLApp.Plugins.Confluence", "Tql.Plugins.Confluence.dll", "0.9.1")]
    [TestCase("TQLApp.Plugins.Jira", "Tql.Plugins.Jira.dll", "0.9.1")]
    [TestCase("TQLApp.Plugins.GitHub", "Tql.Plugins.GitHub.dll", "0.9.1")]
    [TestCase("TQLApp.Plugins.MicrosoftTeams", "Tql.Plugins.MicrosoftTeams.dll", "0.9.1")]
    [TestCase("TQLApp.Plugins.Outlook", "Tql.Plugins.Outlook.dll", "0.9.1")]
    [TestCase("TQLApp.Plugins.Demo", "Tql.Plugins.Demo.dll", "0.9.1")]
    public async Task InstallPackages(string packageId, string dll, string version)
    {
        using var client = CreateClient();

        var packageIdentity = new PackageIdentity(packageId, new NuGetVersion(version));

        var installedPackageIdentities = await client.InstallPackage(
            packageIdentity,
            null,
            NullProgress.Instance
        );

        var files = client.GetPackageFiles(packageIdentity);

        Assert.AreEqual(1, files.Length);
        Assert.AreEqual(dll, Path.GetFileName(files[0]));
        Assert.IsTrue(File.Exists(files[0]));

        foreach (var installedPackageIdentity in installedPackageIdentities)
        {
            foreach (var file in client.GetPackageFiles(installedPackageIdentity))
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
            Constants.PackageSources.Select(p => new NuGetClientSource(p, null)).ToImmutableArray()
        );

        return new NuGetClient(
            configuration,
            new NuGetLogger(),
            Constants.ApplicationFrameworkVersion,
            Constants.SystemPackageIds
        );
    }
}
