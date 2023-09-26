namespace Launcher.Plugins.AzureDevOps.Data;

internal record AzureData(ImmutableArray<AzureConnection> Connections);

internal record AzureConnection(string Name, ImmutableArray<AzureProject> Projects);

internal record AzureProject(string Name, ImmutableArray<AzureRepository> Repositories);

internal record AzureRepository(string Name);
