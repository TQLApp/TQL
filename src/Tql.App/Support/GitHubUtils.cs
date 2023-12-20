using System.Net.Http;
using System.Net.Http.Headers;

namespace Tql.App.Support;

internal static class GitHubUtils
{
    public static void InitializeRequest(HttpRequestMessage request)
    {
        request.Headers.UserAgent.Add(
            new ProductInfoHeaderValue("TQL", GetAppVersion().ToString())
        );
    }

    public static Version GetAppVersion()
    {
        return typeof(GitHubUtils).Assembly.GetName().Version!;
    }
}
