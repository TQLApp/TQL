using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi.FeatureManagement;

namespace Launcher.Plugins.AzureDevOps.Data;

internal class AzureCacheManager : ICacheManager<AzureData>
{
    private readonly IConfigurationManager _configurationManager;
    private readonly IAzureDevOpsApi _api;

    public TimeSpan Expiration => TimeSpan.FromHours(0.5);
    public int Version => 1;

    public AzureCacheManager(IConfigurationManager configurationManager, IAzureDevOpsApi api)
    {
        _configurationManager = configurationManager;
        _api = api;
    }

    public async Task<AzureData> Create()
    {
        var configuration = Configuration.FromJson(
            _configurationManager.GetConfiguration(AzureDevOpsPlugin.Id)
        );

        var connections = new List<AzureConnection>();

        foreach (var connection in configuration.Connections)
        {
            connections.Add(await LoadConnection(connection));
        }

        return new AzureData(connections.ToImmutableArray());
    }

    private async Task<AzureConnection> LoadConnection(Connection connection)
    {
        var features = await GetFeatures(connection);

        var projects = new List<AzureProject>();

        var projectClient = await _api.GetClient<ProjectHttpClient>(connection.Url);

        foreach (var project in await projectClient.GetProjects())
        {
            var workItemTypes = ImmutableArray<AzureWorkItemType>.Empty;
            var dashboards = ImmutableArray<AzureDashboard>.Empty;
            var boards = ImmutableArray<AzureBoard>.Empty;
            var teams = ImmutableArray<AzureTeam>.Empty;
            var repositories = ImmutableArray<AzureRepository>.Empty;
            var buildDefinitions = ImmutableArray<AzureBuildDefinition>.Empty;

            if (features[project.Id].Contains(Feature.Boards))
            {
                var workItemClient = await _api.GetClient<WorkItemTrackingHttpClient>(
                    connection.Url
                );

                workItemTypes = (
                    from workItemType in await workItemClient.GetWorkItemTypesAsync(project.Id)
                    select new AzureWorkItemType(workItemType.Name)
                ).ToImmutableArray();

                var dashboardClient = await _api.GetClient<DashboardHttpClient>(connection.Url);

                dashboards = (
                    from dashboard in (
                        await dashboardClient.GetDashboardsAsync(new TeamContext(project.Id))
                    ).DashboardEntries
                    select new AzureDashboard(dashboard.Id.Value, dashboard.Name)
                ).ToImmutableArray();

                var workClient = await _api.GetClient<WorkHttpClient>(connection.Url);

                boards = (
                    from board in await workClient.GetBoardsAsync(new TeamContext(project.Id))
                    select new AzureBoard(board.Id, board.Name)
                ).ToImmutableArray();

                var teamClient = await _api.GetClient<TeamHttpClient>(connection.Url);

                teams = (
                    from team in await teamClient.GetTeamsAsync(project.Id.ToString())
                    select new AzureTeam(team.Id, team.Name)
                ).ToImmutableArray();
            }

            if (features[project.Id].Contains(Feature.Pipelines))
            {
                var buildClient = await _api.GetClient<BuildHttpClient>(connection.Url);

                buildDefinitions = (
                    from buildDefinition in await buildClient.GetDefinitionsAsync(project.Id)
                    select new AzureBuildDefinition(
                        buildDefinition.Id,
                        buildDefinition.Name,
                        buildDefinition.Path
                    )
                ).ToImmutableArray();
            }

            if (features[project.Id].Contains(Feature.Repositories))
            {
                var gitClient = await _api.GetClient<GitHttpClient>(connection.Url);

                repositories = (
                    from repository in await gitClient.GetRepositoriesAsync(project.Id)
                    select new AzureRepository(repository.Id, repository.Name)
                ).ToImmutableArray();
            }

            projects.Add(
                new AzureProject(
                    project.Id,
                    project.Name,
                    workItemTypes,
                    dashboards,
                    boards,
                    teams,
                    repositories,
                    buildDefinitions
                )
            );
        }

        return new AzureConnection(connection.Name, projects.ToImmutableArray());
    }

    private async Task<Dictionary<Guid, HashSet<Feature>>> GetFeatures(Connection connection)
    {
        var featureClient = await _api.GetClient<FeatureManagementHttpClient>(connection.Url);

        var featureIds = (
            from feature in await featureClient.GetFeaturesAsync()
            where feature.Scopes.Any(p => p.SettingScope == "project")
            select feature.Id
        ).ToList();

        var projectClient = await _api.GetClient<ProjectHttpClient>(connection.Url);

        var result = new Dictionary<Guid, HashSet<Feature>>();

        foreach (var project in await projectClient.GetProjects())
        {
            // From https://stackoverflow.com/questions/55425181/how-to-enable-disable-project-services-through-api.

            var state = await featureClient.QueryFeatureStatesForNamedScopeAsync(
                new ContributedFeatureStateQuery
                {
                    ScopeValues = new Dictionary<string, string>
                    {
                        ["settingScope"] = "project",
                        ["userScope"] = "false"
                    },
                    FeatureIds = featureIds
                },
                "host",
                "project",
                project.Id.ToString()
            );

            var projectFeatures = new HashSet<Feature>();

            foreach (var feature in state.FeatureStates)
            {
                if (feature.Value.State != ContributedFeatureEnabledValue.Disabled)
                {
                    switch (feature.Key)
                    {
                        case "ms.vss-code.version-control":
                            projectFeatures.Add(Feature.Repositories);
                            break;
                        case "ms.vss-work.agile":
                            projectFeatures.Add(Feature.Boards);
                            break;
                        case "ms.vss-build.pipelines":
                            projectFeatures.Add(Feature.Pipelines);
                            break;
                    }
                }
            }

            result.Add(project.Id, projectFeatures);
        }

        return result;
    }

    private enum Feature
    {
        Repositories,
        Boards,
        Pipelines
    }
}
