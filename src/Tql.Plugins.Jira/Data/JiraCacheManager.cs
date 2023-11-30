using System.Globalization;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Logging;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Data;

internal class JiraCacheManager : ICacheManager<JiraData>
{
    private readonly ConfigurationManager _configurationManager;
    private readonly JiraApi _api;
    private readonly ILogger<JiraCacheManager> _logger;

    public int Version => 6;

    public event EventHandler<CacheInvalidationRequiredEventArgs>? CacheInvalidationRequired;

    public JiraCacheManager(
        ConfigurationManager configurationManager,
        JiraApi api,
        ILogger<JiraCacheManager> logger
    )
    {
        _configurationManager = configurationManager;
        _api = api;
        _logger = logger;

        configurationManager.Changed += (_, _) =>
            OnCacheInvalidationRequired(new CacheInvalidationRequiredEventArgs(true));
    }

    public async Task<JiraData> Create()
    {
        var results = ImmutableArray.CreateBuilder<JiraConnection>();

        foreach (var connection in _configurationManager.Configuration.Connections)
        {
            results.Add(await CreateConnection(connection));
        }

        return new JiraData(results.ToImmutable());
    }

    private async Task<JiraConnection> CreateConnection(Connection connection)
    {
        var client = _api.GetClient(connection);

        var dashboards = (
            from dashboard in await client.GetDashboards()
            select new JiraDashboard(dashboard.Id, dashboard.Name, dashboard.View)
        ).ToImmutableArray();

        var projects = ImmutableArray.CreateBuilder<JiraProject>();

        foreach (var project in await client.GetProjects())
        {
            var issueTypes = ImmutableArray<JiraIssueType>.Empty;
            if (project.IssueTypes != null)
            {
                issueTypes = project
                    .IssueTypes
                    .Value
                    .Select(
                        p => new JiraIssueType(p.Id, p.Description, p.IconUrl, p.Name, p.SubTask)
                    )
                    .ToImmutableArray();
            }

            projects.Add(
                new JiraProject(
                    project.Id,
                    project.Key,
                    project.Name,
                    SelectAvatarUrl(project),
                    project.ProjectTypeKey,
                    project.Simplified,
                    project.Style,
                    issueTypes
                )
            );
        }

        var boards = await GetAgileBoards(client);

        ImmutableArray<JiraFilterDto> apiFilters;

        // Fall back to the server APIs if we can't get all filters through the
        // cloud APIs.
        try
        {
            apiFilters = await client.GetFiltersV3();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get all filters");

            apiFilters = await client.GetFavoriteFilters();
        }

        var filters = apiFilters
            .Select(p => new JiraFilter(p.Id, p.Name, p.Jql, p.ViewUrl))
            .ToImmutableArray();

        return new JiraConnection(
            connection.Url,
            dashboards,
            projects.ToImmutable(),
            boards,
            filters
        );
    }

    private async Task<ImmutableArray<JiraBoard>> GetAgileBoards(JiraClient client)
    {
        try
        {
            var boards = ImmutableArray.CreateBuilder<JiraBoard>();

            foreach (var board in await client.GetAgileBoards())
            {
                var location = board.Location;
                if (location == null)
                    continue;

                var boardConfig = await client.GetAgileBoardsConfiguration(board.Id);
                var xBoardConfig = await client.GetXBoardConfig(board.Id);

                boards.Add(
                    new JiraBoard(
                        board.Id,
                        board.Name,
                        board.Type,
                        location.Name,
                        location.ProjectKey,
                        location.ProjectTypeKey,
                        location.AvatarUri,
                        boardConfig.Filter.Id,
                        xBoardConfig.CurrentViewConfig.IsIssueListBacklog,
                        xBoardConfig.CurrentViewConfig.SprintSupportEnabled,
                        xBoardConfig
                            .CurrentViewConfig
                            .QuickFilters
                            .Select(p => new JiraQuickFilter(p.Id, p.Name, p.Query))
                            .ToImmutableArray()
                    )
                );
            }

            return boards.ToImmutable();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger.LogWarning(ex, "Failed to get access to JIRA boards");

            return ImmutableArray<JiraBoard>.Empty;
        }
    }

    private string SelectAvatarUrl(JiraProjectDto project)
    {
        return project
            .AvatarUrls
            .Select(p => (Size: GetSize(p.Key), Url: p.Value))
            .MaxBy(p => p.Size)
            .Url;

        int GetSize(string key) =>
            int.Parse(key.Substring(0, key.IndexOf('x')), CultureInfo.InvariantCulture);
    }

    protected virtual void OnCacheInvalidationRequired(CacheInvalidationRequiredEventArgs e) =>
        CacheInvalidationRequired?.Invoke(this, e);
}
