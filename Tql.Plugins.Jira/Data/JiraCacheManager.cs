using Microsoft.Extensions.Logging;
using System.Globalization;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Data;

internal class JiraCacheManager : ICacheManager<JiraData>
{
    private readonly ConfigurationManager _configurationManager;
    private readonly JiraApi _api;
    private readonly ILogger<JiraCacheManager> _logger;

    public int Version => 1;

    public JiraCacheManager(
        ConfigurationManager configurationManager,
        JiraApi api,
        ILogger<JiraCacheManager> logger
    )
    {
        _configurationManager = configurationManager;
        _api = api;
        _logger = logger;
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

        var projects = (
            from project in await client.GetProjects()
            select new JiraProject(
                project.Id,
                project.Key,
                project.Name,
                SelectAvatarUrl(project),
                project.ProjectTypeKey
            )
        ).ToImmutableArray();

        var boards = ImmutableArray<JiraBoard>.Empty;

        try
        {
            boards = (
                from board in await client.GetBoardsV3()
                select new JiraBoard(
                    board.Id,
                    board.Name,
                    board.Type,
                    board.Location.Name,
                    board.Location.ProjectKey,
                    board.Location.ProjectTypeKey,
                    board.Location.AvatarUri
                )
            ).ToImmutableArray();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load boards");
        }

        return new JiraConnection(connection.Url, dashboards, projects, boards);
    }

    private string SelectAvatarUrl(JiraProjectDto project)
    {
        return project.AvatarUrls
            .Select(p => (Size: GetSize(p.Key), Url: p.Value))
            .OrderByDescending(p => p.Size)
            .First()
            .Url;

        int GetSize(string key) =>
            int.Parse(key.Substring(0, key.IndexOf('x')), CultureInfo.InvariantCulture);
    }
}
