namespace Tql.Plugins.AzureDevOps.Data;

internal record AzureData(ImmutableArray<AzureConnection> Connections)
{
    public AzureConnection GetConnection(string url)
    {
        return Connections.Single(p => p.Url == url);
    }
};

internal record AzureConnection(string Name, string Url, ImmutableArray<AzureProject> Projects);

internal record AzureProject(
    Guid Id,
    string Name,
    ImmutableArray<AzureFeature> Features,
    ImmutableArray<AzureWorkItemType> WorkItemTypes,
    ImmutableArray<AzureDashboard> Dashboards,
    ImmutableArray<AzureBacklog> Backlogs,
    ImmutableArray<AzureBoard> Boards,
    ImmutableArray<AzureTeam> Teams,
    ImmutableArray<AzureRepository> Repositories,
    ImmutableArray<AzureBuildDefinition> BuildDefinitions
);

internal enum AzureFeature
{
    Repositories,
    Boards,
    Pipelines
}

internal record AzureWorkItemType(string Name, AzureWorkItemIcon Icon);

internal record AzureWorkItemIcon(byte[] Data, string? MediaType);

internal record AzureDashboard(Guid Id, string Name);

internal record AzureBacklog(string Name, ImmutableArray<string> WorkItemTypes);

internal record AzureBoard(Guid Id, string Name);

internal record AzureTeam(Guid Id, string Name, ImmutableArray<AzureTeamAreaPath> AreaPaths);

internal record AzureTeamAreaPath(string Name, bool IncludeChildren);

internal record AzureRepository(Guid Id, string Name);

internal record AzureBuildDefinition(int Id, string Name, string Path);
