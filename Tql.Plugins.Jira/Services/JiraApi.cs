using System.Net.Http;

namespace Tql.Plugins.Jira.Services;

internal class JiraApi
{
    private readonly HttpClient _httpClient;
    private readonly ConnectionManager _connectionManager;

    public JiraApi(HttpClient httpClient, ConnectionManager connectionManager)
    {
        _httpClient = httpClient;
        _connectionManager = connectionManager;
    }

    public JiraClient GetClient(string url)
    {
        var connection = _connectionManager.Connections.Single(p => p.Url == url);
        return GetClient(connection);
    }

    public JiraClient GetClient(Connection connection)
    {
        return new JiraClient(_httpClient, connection);
    }
}
