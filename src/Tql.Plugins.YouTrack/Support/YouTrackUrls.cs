using System;
using System.Collections.Immutable;

namespace Tql.Plugins.YouTrack.Support;

// Service URLs and the links the plugin opens. A service URL is the address
// the user opens YouTrack on, kept with one trailing slash; the REST API is
// "<service URL>api/" and every link is built from it, so an installation
// under a path prefix needs no special handling.
//
// This file has no WPF or TQL dependencies, so the plain net8.0 test project
// can compile it.
internal static class YouTrackUrls
{
    /// <summary>
    /// Turns what the user entered into a service URL: an absolute http or
    /// https URL without query or fragment, with one trailing slash. A bare
    /// host gets https.
    /// </summary>
    /// <exception cref="ArgumentException">The URL is not an http or https URL.</exception>
    public static string NormalizeServiceUrl(string url)
    {
        var text = url.Trim();
        if (!text.Contains("://"))
            text = "https://" + text;

        if (
            !Uri.TryCreate(text, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
            || uri.Host.Length == 0
        )
        {
            throw new ArgumentException($"'{url}' is not an http or https URL.", nameof(url));
        }

        return $"{uri.Scheme}://{uri.Authority}{uri.AbsolutePath.TrimEnd('/')}/";
    }

    /// <summary>
    /// The service URL followed by the URLs with its path segments removed one
    /// at a time, down to the bare host. The connection check tries them in
    /// this order.
    /// </summary>
    public static ImmutableArray<string> GetServiceUrlCandidates(string serviceUrl)
    {
        var url = NormalizeServiceUrl(serviceUrl);
        var uri = new Uri(url);
        var root = $"{uri.Scheme}://{uri.Authority}/";

        var result = ImmutableArray.CreateBuilder<string>();

        result.Add(url);

        while (url != root)
        {
            var path = url.Substring(root.Length).TrimEnd('/');
            var index = path.LastIndexOf('/');

            url = index < 0 ? root : root + path.Substring(0, index + 1);

            result.Add(url);
        }

        return result.ToImmutable();
    }

    /// <summary>
    /// Resolves a URL the API returned, such as the host-relative
    /// "/hub/api/rest/avatar/...", against the service URL by the normal URI
    /// rules. An absolute URL is returned as is.
    /// </summary>
    public static string Resolve(string serviceUrl, string url)
    {
        return new Uri(new Uri(WithTrailingSlash(serviceUrl)), url).AbsoluteUri;
    }

    /// <summary>
    /// Whether the URL has the service URL's scheme, host and port: the only
    /// place the token may be sent.
    /// </summary>
    public static bool IsSameOrigin(string serviceUrl, string url)
    {
        var service = new Uri(WithTrailingSlash(serviceUrl));

        return Uri.TryCreate(url, UriKind.Absolute, out var other)
            && string.Equals(service.Scheme, other.Scheme, StringComparison.OrdinalIgnoreCase)
            && string.Equals(service.Host, other.Host, StringComparison.OrdinalIgnoreCase)
            && service.Port == other.Port;
    }

    public static string GetIssueUrl(string serviceUrl, string idReadable) =>
        $"{WithTrailingSlash(serviceUrl)}issue/{Uri.EscapeDataString(idReadable)}";

    public static string GetProjectUrl(string serviceUrl, string shortName) =>
        $"{WithTrailingSlash(serviceUrl)}projects/{Uri.EscapeDataString(shortName)}";

    public static string GetAgileBoardUrl(string serviceUrl, string id) =>
        $"{WithTrailingSlash(serviceUrl)}agiles/{Uri.EscapeDataString(id)}/current";

    public static string GetSavedSearchUrl(string serviceUrl, string query) =>
        $"{WithTrailingSlash(serviceUrl)}issues?q={Uri.EscapeDataString(query)}";

    public static string GetDashboardUrl(string serviceUrl, string id) =>
        $"{WithTrailingSlash(serviceUrl)}dashboard?id={Uri.EscapeDataString(id)}";

    public static string GetNewIssueUrl(string serviceUrl, string shortName) =>
        $"{WithTrailingSlash(serviceUrl)}newIssue?project={Uri.EscapeDataString(shortName)}";

    private static string WithTrailingSlash(string serviceUrl) => serviceUrl.TrimEnd('/') + "/";
}
