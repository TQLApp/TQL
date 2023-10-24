using System.Globalization;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;

namespace Tql.Plugins.Jira.Data;

internal class JiraCacheManager : ICacheManager<JiraData>
{
    private readonly ConnectionManager _connectionManager;
    private readonly JiraApi _api;

    public int Version => 1;

    public JiraCacheManager(ConnectionManager connectionManager, JiraApi api)
    {
        _connectionManager = connectionManager;
        _api = api;
    }

    public async Task<JiraData> Create()
    {
        var results = ImmutableArray.CreateBuilder<JiraConnection>();

        foreach (var connection in _connectionManager.Connections)
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

        return new JiraConnection(connection.Url, dashboards, projects);
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
