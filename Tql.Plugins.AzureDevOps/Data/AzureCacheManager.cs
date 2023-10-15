using System.IO;
using System.Net.Http;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.TeamFoundation.Core.WebApi.Types;
using Microsoft.TeamFoundation.Dashboards.WebApi;
using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.TeamFoundation.Work.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Microsoft.VisualStudio.Services.ExtensionManagement.WebApi.FeatureManagement;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;

namespace Tql.Plugins.AzureDevOps.Data;

internal class AzureCacheManager : ICacheManager<AzureData>
{
    private readonly IConfigurationManager _configurationManager;
    private readonly AzureDevOpsApi _api;
    private readonly HttpClient _httpClient;

    public TimeSpan Expiration => TimeSpan.FromHours(0.5);
    public int Version => 1;

    public AzureCacheManager(
        IConfigurationManager configurationManager,
        AzureDevOpsApi api,
        HttpClient httpClient
    )
    {
        _configurationManager = configurationManager;
        _api = api;
        _httpClient = httpClient;
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
            var backlogs = ImmutableArray<AzureBacklog>.Empty;
            var boards = ImmutableArray<AzureBoard>.Empty;
            var teams = ImmutableArray<AzureTeam>.Empty;
            var repositories = ImmutableArray<AzureRepository>.Empty;
            var buildDefinitions = ImmutableArray<AzureBuildDefinition>.Empty;

            if (features[project.Id].Contains(AzureFeature.Boards))
            {
                var workItemClient = await _api.GetClient<WorkItemTrackingHttpClient>(
                    connection.Url
                );

                var workItemTypeList = new List<AzureWorkItemType>();

                foreach (var workItemType in await workItemClient.GetWorkItemTypesAsync(project.Id))
                {
                    var icon = await CreateWorkItemIcon(workItemType.Icon);
                    workItemTypeList.Add(new AzureWorkItemType(workItemType.Name, icon));
                }

                workItemTypes = workItemTypeList.ToImmutableArray();

                var dashboardClient = await _api.GetClient<DashboardHttpClient>(connection.Url);

                dashboards = (
                    from dashboard in (
                        await dashboardClient.GetDashboardsByProjectAsync(
                            new TeamContext(project.Id)
                        )
                    )
                    select new AzureDashboard(dashboard.Id!.Value, dashboard.Name)
                ).ToImmutableArray();

                var workClient = await _api.GetClient<WorkHttpClient>(connection.Url);

                boards = (
                    from board in await workClient.GetBoardsAsync(new TeamContext(project.Id))
                    select new AzureBoard(board.Id, board.Name)
                ).ToImmutableArray();

                var backlogConfigurations = await workClient.GetBacklogConfigurationsAsync(
                    new TeamContext(project.Id)
                );

                backlogs = (
                    from backlog in backlogConfigurations.PortfolioBacklogs.Concat(
                        new[]
                        {
                            backlogConfigurations.RequirementBacklog,
                            backlogConfigurations.TaskBacklog
                        }
                    )
                    where backlog != null
                    select new AzureBacklog(backlog.Name)
                ).ToImmutableArray();

                var teamClient = await _api.GetClient<TeamHttpClient>(connection.Url);

                teams = (
                    from team in await teamClient.GetTeamsAsync(project.Id.ToString())
                    select new AzureTeam(team.Id, team.Name)
                ).ToImmutableArray();
            }

            if (features[project.Id].Contains(AzureFeature.Pipelines))
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

            if (features[project.Id].Contains(AzureFeature.Repositories))
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
                    features[project.Id].ToImmutableArray(),
                    workItemTypes,
                    dashboards,
                    backlogs,
                    boards,
                    teams,
                    repositories,
                    buildDefinitions
                )
            );
        }

        return new AzureConnection(connection.Name, connection.Url, projects.ToImmutableArray());
    }

    private async Task<AzureWorkItemIcon> CreateWorkItemIcon(WorkItemIcon icon)
    {
        using var response = await _httpClient.GetAsync(icon.Url);

        response.EnsureSuccessStatusCode();

        using var source = await response.Content.ReadAsStreamAsync();
        using var target = new MemoryStream();

        await source.CopyToAsync(target);

        return new AzureWorkItemIcon(
            target.ToArray(),
            response.Content.Headers.ContentType.MediaType
        );
    }

    private async Task<Dictionary<Guid, HashSet<AzureFeature>>> GetFeatures(Connection connection)
    {
        var featureClient = await _api.GetClient<FeatureManagementHttpClient>(connection.Url);

        var featureIds = (
            from feature in await featureClient.GetFeaturesAsync()
            where feature.Scopes.Any(p => p.SettingScope == "project")
            select feature.Id
        ).ToList();

        var projectClient = await _api.GetClient<ProjectHttpClient>(connection.Url);

        var result = new Dictionary<Guid, HashSet<AzureFeature>>();

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

            var projectFeatures = new HashSet<AzureFeature>();

            foreach (var feature in state.FeatureStates)
            {
                if (feature.Value.State != ContributedFeatureEnabledValue.Disabled)
                {
                    switch (feature.Key)
                    {
                        case "ms.vss-code.version-control":
                            projectFeatures.Add(AzureFeature.Repositories);
                            break;
                        case "ms.vss-work.agile":
                            projectFeatures.Add(AzureFeature.Boards);
                            break;
                        case "ms.vss-build.pipelines":
                            projectFeatures.Add(AzureFeature.Pipelines);
                            break;
                    }
                }
            }

            result.Add(project.Id, projectFeatures);
        }

        return result;
    }
}
