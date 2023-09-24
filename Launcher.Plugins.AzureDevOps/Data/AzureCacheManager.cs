using Launcher.Abstractions;

namespace Launcher.Plugins.AzureDevOps.Data;

internal class AzureCacheManager : ICacheManager<AzureData>
{
    public TimeSpan Expiration => TimeSpan.FromHours(0.5);
    public int Version => 1;

    public Task<AzureData> Create()
    {
        return Task.FromResult(
            new AzureData(
                ImmutableArray.Create(
                    new AzureProject(
                        "Test",
                        ImmutableArray.Create<AzureRepository>(new AzureRepository("Test"))
                    )
                )
            )
        );
    }
}
