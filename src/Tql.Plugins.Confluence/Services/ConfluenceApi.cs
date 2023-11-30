using System.Net.Http;
using Tql.Abstractions;

namespace Tql.Plugins.Confluence.Services;

internal class ConfluenceApi(HttpClient httpClient, IUI ui, IEncryption encryption)
{
    public ConfluenceClient GetClient(Connection connection)
    {
        return new ConfluenceClient(httpClient, connection, ui, encryption);
    }
}
