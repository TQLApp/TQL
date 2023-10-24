using System.Net.Http;

namespace Tql.Plugins.Jira.Services;

internal class JiraApi
{
    private readonly HttpClient _httpClient;

    public JiraApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public JiraClient GetClient(Connection connection)
    {
        return new JiraClient(_httpClient, connection);
    }
}
