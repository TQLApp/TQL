using Atlassian.Jira;
using Atlassian.Jira.Remote;
using NeoSmart.AsyncLock;
using RestSharp;
using RestSharp.Authenticators;
using Tql.Plugins.Jira.Support;

namespace Tql.Plugins.Jira.Services;

internal class JiraApi
{
    private readonly ConnectionManager _connectionManager;
    private readonly AsyncLock _lock = new();
    private readonly Dictionary<Guid, Atlassian.Jira.Jira> _clients = new();

    public JiraApi(ConnectionManager connectionManager)
    {
        _connectionManager = connectionManager;
    }

    public async Task<Atlassian.Jira.Jira> GetClient(Guid id)
    {
        using (await _lock.LockAsync())
        {
            if (!_clients.TryGetValue(id, out var client))
            {
                var connection = _connectionManager.Connections.Single(p => p.Id == id);

                client = CreateClient(connection);

                _clients[id] = client;
            }

            return client;
        }
    }

    public static Atlassian.Jira.Jira CreateClient(Connection connection)
    {
        return Atlassian.Jira.Jira.CreateRestClient(
            new MyJiraRestClient(
                connection.Url,
                new HttpBearerAuthenticator(
                    Encryption.Unprotect(connection.ProtectedPatToken)
                        ?? throw new InvalidOperationException("Could not read PAT token")
                )
            )
        );
    }

    private class MyJiraRestClient : JiraRestClient
    {
        public MyJiraRestClient(
            string url,
            IAuthenticator authenticator,
            JiraRestClientSettings? settings = null
        )
            : base(url, authenticator, settings) { }
    }

    private class HttpBearerAuthenticator : AuthenticatorBase
    {
        public HttpBearerAuthenticator(string patToken)
            : base(patToken) { }

#pragma warning disable CS0618 // Type or member is obsolete
        protected override Parameter GetAuthenticationParameter(string accessToken) =>
            new Parameter("Authorization", $"Bearer {accessToken}", ParameterType.HttpHeader);
#pragma warning restore CS0618 // Type or member is obsolete
    }
}
