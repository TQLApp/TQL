using Tql.Utilities;

namespace Tql.App.Services.Packages;

internal record PackageManagerConfiguration(ImmutableArray<PackageManagerSource> Sources)
{
    public static readonly PackageManagerConfiguration Default = CreateDefault();

    private static PackageManagerConfiguration CreateDefault()
    {
        var sources = ImmutableArray.CreateBuilder<PackageManagerSource>();

        sources.Add(new PackageManagerSource("https://api.nuget.org/v3/index.json", null, null));

        var betaArtifactFeedUserName = Environment.GetEnvironmentVariable(
            "AzureDevOpsArtifactsUserName"
        );
        var betaArtifactFeedPatToken = Environment.GetEnvironmentVariable(
            "AzureDevOpsArtifactsPatToken"
        );

        if (
            !string.IsNullOrEmpty(betaArtifactFeedUserName)
            && !string.IsNullOrEmpty(betaArtifactFeedPatToken)
        )
        {
            sources.Add(
                new PackageManagerSource(
                    "https://pvginkel.pkgs.visualstudio.com/Launcher/_packaging/TQLPlugins/nuget/v3/index.json",
                    betaArtifactFeedUserName,
                    Encryption.Protect(betaArtifactFeedPatToken)
                )
            );
        }

        return new PackageManagerConfiguration(sources.ToImmutable());
    }

    public static PackageManagerConfiguration FromJson(string? json)
    {
        if (json == null)
            return Default;

        return JsonSerializer.Deserialize<PackageManagerConfiguration>(json)!;
    }

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}

internal record PackageManagerSource(string Url, string? UserName, byte[]? ProtectedPassword);
