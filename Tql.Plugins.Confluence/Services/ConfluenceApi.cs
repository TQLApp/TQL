using System.Net.Http;

namespace Tql.Plugins.Confluence.Services;

internal class ConfluenceApi
{
    private readonly HttpClient _httpClient;

    public ConfluenceApi(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public ConfluenceClient GetClient(Connection connection)
    {
        return new ConfluenceClient(_httpClient, connection);
    }
}
