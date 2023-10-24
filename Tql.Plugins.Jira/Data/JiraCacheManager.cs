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

        var dashboards = await GetDashboards(client);

        return new JiraConnection(connection.Url, dashboards);
    }

    private async Task<ImmutableArray<JiraDashboard>> GetDashboards(JiraClient client)
    {
        var dashboards = await client.GetDashboards();

        return dashboards.Select(p => new JiraDashboard(p.Id, p.Name, p.View)).ToImmutableArray();
    }
}
