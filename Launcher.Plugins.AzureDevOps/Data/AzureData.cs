namespace Launcher.Plugins.AzureDevOps.Data;

internal record AzureData(ImmutableArray<AzureConnection> Connections);

internal record AzureConnection(string Name, ImmutableArray<AzureProject> Projects);

internal record AzureProject(
    Guid Id,
    string Name,
    ImmutableArray<AzureWorkItemType> WorkItemTypes,
    ImmutableArray<AzureDashboard> Dashboards,
    ImmutableArray<AzureBoard> Boards,
    ImmutableArray<AzureTeam> Teams,
    ImmutableArray<AzureRepository> Repositories,
    ImmutableArray<AzureBuildDefinition> BuildDefinitions
);

internal record AzureWorkItemType(string Name);

internal record AzureDashboard(Guid Id, string Name);

internal record AzureBoard(Guid Id, string Name);

internal record AzureTeam(Guid Id, string Name);

internal record AzureRepository(Guid Id, string Name);

internal record AzureBuildDefinition(int Id, string Name, string Path);
