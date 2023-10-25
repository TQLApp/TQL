using System.Net.Http;
using Tql.Abstractions;

namespace Tql.Plugins.Jira.Services;

internal class JiraApi
{
    private readonly HttpClient _httpClient;
    private readonly IUI _ui;

    public JiraApi(HttpClient httpClient, IUI ui)
    {
        _httpClient = httpClient;
        _ui = ui;
    }

    public JiraClient GetClient(Connection connection)
    {
        return new JiraClient(_httpClient, connection, _ui);
    }
}
