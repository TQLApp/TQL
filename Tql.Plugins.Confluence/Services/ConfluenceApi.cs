using System.Net.Http;
using Tql.Abstractions;

namespace Tql.Plugins.Confluence.Services;

internal class ConfluenceApi
{
    private readonly HttpClient _httpClient;
    private readonly IUI _ui;

    public ConfluenceApi(HttpClient httpClient, IUI ui)
    {
        _httpClient = httpClient;
        _ui = ui;
    }

    public ConfluenceClient GetClient(Connection connection)
    {
        return new ConfluenceClient(_httpClient, connection, _ui);
    }
}
