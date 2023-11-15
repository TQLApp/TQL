using System.Net.Http;
using Tql.Abstractions;

namespace Tql.Plugins.Jira.Services;

internal class JiraApi
{
    private readonly HttpClient _httpClient;
    private readonly IUI _ui;
    private readonly IEncryption _encryption;

    public JiraApi(HttpClient httpClient, IUI ui, IEncryption encryption)
    {
        _httpClient = httpClient;
        _ui = ui;
        _encryption = encryption;
    }

    public JiraClient GetClient(Connection connection)
    {
        return new JiraClient(_httpClient, connection, _ui, _encryption);
    }
}
