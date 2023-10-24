using Atlassian.Jira;
using Atlassian.Jira.Remote;
using RestSharp;
using RestSharp.Authenticators;

namespace Tql.Plugins.Jira.Support;

internal static class ConnectionExtensions
{
    public static Atlassian.Jira.Jira CreateClient(this Connection self)
    {
        return Atlassian.Jira.Jira.CreateRestClient(
            new MyJiraRestClient(
                self.Url,
                new HttpBearerAuthenticator(
                    Encryption.Unprotect(self.ProtectedPatToken)
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
