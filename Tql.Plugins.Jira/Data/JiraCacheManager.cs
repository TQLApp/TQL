using System.Globalization;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Data;

internal class JiraCacheManager : ICacheManager<JiraData>
{
    private readonly ConfigurationManager _configurationManager;
    private readonly JiraApi _api;

    public int Version => 4;

    public event EventHandler<CacheExpiredEventArgs>? CacheExpired;

    public JiraCacheManager(ConfigurationManager configurationManager, JiraApi api)
    {
        _configurationManager = configurationManager;
        _api = api;

        configurationManager.Changed += (_, _) => OnCacheExpired(new CacheExpiredEventArgs(true));
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
                project.ProjectTypeKey,
                project.Simplified,
                project.Style
            )
        ).ToImmutableArray();

        var boards = ImmutableArray.CreateBuilder<JiraBoard>();

        foreach (var board in await client.GetBoardsV3())
        {
            var location = board.Location;
            if (location == null)
                continue;

            var boardConfig = await client.GetBoardsConfigurationV3(board.Id);
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
                    xBoardConfig.CurrentViewConfig.QuickFilters
                        .Select(p => new JiraQuickFilter(p.Id, p.Name, p.Query))
                        .ToImmutableArray()
                )
            );
        }

        return new JiraConnection(connection.Url, dashboards, projects, boards.ToImmutable());
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

    protected virtual void OnCacheExpired(CacheExpiredEventArgs e) => CacheExpired?.Invoke(this, e);
}
