using System.Net.Http;
using Tql.Abstractions;

namespace Tql.Plugins.Jira.Services;

internal class JiraApi(HttpClient httpClient, IUI ui, IEncryption encryption)
{
    public JiraClient GetClient(Connection connection)
    {
        return new JiraClient(httpClient, connection, ui, encryption);
    }
}
