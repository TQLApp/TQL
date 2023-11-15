using System.Net.Http;
using Tql.Abstractions;

namespace Tql.Plugins.Confluence.Services;

internal class ConfluenceApi
{
    private readonly HttpClient _httpClient;
    private readonly IUI _ui;
    private readonly IEncryption _encryption;

    public ConfluenceApi(HttpClient httpClient, IUI ui, IEncryption encryption)
    {
        _httpClient = httpClient;
        _ui = ui;
        _encryption = encryption;
    }

    public ConfluenceClient GetClient(Connection connection)
    {
        return new ConfluenceClient(_httpClient, connection, _ui, _encryption);
    }
}
