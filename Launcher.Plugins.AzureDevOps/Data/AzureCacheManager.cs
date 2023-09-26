using Launcher.Abstractions;

namespace Launcher.Plugins.AzureDevOps.Data;

internal class AzureCacheManager : ICacheManager<AzureData>
{
    private readonly IConfigurationManager _configurationManager;

    public TimeSpan Expiration => TimeSpan.FromHours(0.5);
    public int Version => 1;

    public AzureCacheManager(IConfigurationManager configurationManager)
    {
        _configurationManager = configurationManager;
    }

    public Task<AzureData> Create()
    {
        var configuration = Configuration.FromJson(
            _configurationManager.GetConfiguration(AzureDevOpsPlugin.Id)
        );

        var connections = new List<AzureConnection>();

        foreach (var connection in configuration.Connections)
        {
            connections.Add(
                new AzureConnection(
                    connection.Name,
                    ImmutableArray.Create(
                        new AzureProject(
                            "Test",
                            ImmutableArray.Create<AzureRepository>(new AzureRepository("Test"))
                        )
                    )
                )
            );
        }

        return Task.FromResult(new AzureData(connections.ToImmutableArray()));
    }
}
