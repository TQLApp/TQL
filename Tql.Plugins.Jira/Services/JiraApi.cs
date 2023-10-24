using System.Net.Http;

namespace Tql.Plugins.Jira.Services;

internal class JiraApi
{
    private readonly HttpClient _httpClient;
    private readonly ConfigurationManager _configurationManager;

    public JiraApi(HttpClient httpClient, ConfigurationManager configurationManager)
    {
        _httpClient = httpClient;
        _configurationManager = configurationManager;
    }

    public JiraClient GetClient(string url)
    {
        var connection = _configurationManager.Configuration.Connections.Single(p => p.Url == url);
        return GetClient(connection);
    }

    public JiraClient GetClient(Connection connection)
    {
        return new JiraClient(_httpClient, connection);
    }
}
