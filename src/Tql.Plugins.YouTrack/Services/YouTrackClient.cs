using System;
using System.Collections.Immutable;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Tql.Plugins.YouTrack.Support;

namespace Tql.Plugins.YouTrack.Services;

// The REST client for one YouTrack connection, authenticated with a permanent
// token. Every list call passes $top, because YouTrack picks a page size of its
// own when it's left out; the cached lists page with $skip and $top until a
// page comes back short.
//
// This file has no WPF or TQL dependencies, so the plain net8.0 test project
// can compile it. The plugin shows the "unable to connect" notification bar
// through the response observer, and maps the exceptions below to its labels.
internal class YouTrackClient
{
    public const int DefaultPageSize = 500;
    public const int DefaultSearchTop = 100;

    // Only what the plugin displays or links to.
    public const string ConfigFields = "version";
    public const string UserFields = "id,login,fullName,email,banned,guest";
    public const string IssueFields = "idReadable,summary";
    public const string ProjectFields = "id,shortName,name,archived,iconUrl";
    public const string AgileBoardFields = "id,name";
    public const string SavedQueryFields = "id,name,query";
    public const string DashboardFields = "id,name";

    private readonly HttpClient _httpClient;
    private readonly AuthenticationHeaderValue _authorization;
    private readonly Action<HttpStatusCode>? _responseObserver;

    /// <summary>The service URL, with one trailing slash.</summary>
    public string ServiceUrl { get; }

    /// <summary>The $top of every page of the cached lists.</summary>
    public int PageSize { get; init; } = DefaultPageSize;

    /// <param name="httpClient">The HTTP client to send every request with.</param>
    /// <param name="serviceUrl">The address YouTrack is opened on.</param>
    /// <param name="token">The permanent token, decrypted.</param>
    /// <param name="responseObserver">
    /// Called with the status code of every response to a request that
    /// carried the token, e.g. to show a notification bar on a 401 and remove
    /// it on anything else.
    /// </param>
    public YouTrackClient(
        HttpClient httpClient,
        string serviceUrl,
        string token,
        Action<HttpStatusCode>? responseObserver = null
    )
    {
        _httpClient = httpClient;
        _authorization = new AuthenticationHeaderValue("Bearer", token);
        _responseObserver = responseObserver;

        ServiceUrl = YouTrackUrls.NormalizeServiceUrl(serviceUrl);
    }

    /// <summary>
    /// The first step of the connection check, without a token: finds the URL
    /// whose "api/config" answers YouTrack's FrontendConfig, trying the
    /// entered URL and then its parent paths down to the bare host. A
    /// self-hosted YouTrack answers an unknown path with its web app, so a URL
    /// copied from the address bar answers HTML rather than a 404.
    /// </summary>
    /// <exception cref="ArgumentException">The URL is not an http or https URL.</exception>
    /// <exception cref="YouTrackNotAnApiException">
    /// Neither the URL nor any of its parent paths answers as YouTrack.
    /// </exception>
    /// <exception cref="HttpRequestException">The host can't be reached.</exception>
    public static async Task<YouTrackServiceCheck> CheckServiceUrl(
        HttpClient httpClient,
        string url,
        CancellationToken cancellationToken = default
    )
    {
        var enteredUrl = YouTrackUrls.NormalizeServiceUrl(url);
        var enteredApiUrl = default(string);
        var enteredAnswer = default(string);
        var enteredStatusCode = default(HttpStatusCode?);

        foreach (var candidate in YouTrackUrls.GetServiceUrlCandidates(enteredUrl))
        {
            var apiUrl = $"{candidate}api/config?fields={ConfigFields}";

            using var request = CreateRequest(apiUrl);
            using var response = await httpClient.SendAsync(request, cancellationToken);

            var json = await ReadJson(response, cancellationToken);
            var config = json != null ? TryDeserialize<YouTrackConfigDto>(json) : null;

            if (response.IsSuccessStatusCode && config?.Type == "FrontendConfig")
                return new YouTrackServiceCheck(enteredUrl, candidate, config.Version);

            if (enteredApiUrl == null)
            {
                enteredApiUrl = apiUrl;
                enteredAnswer = DescribeAnswer(response);
                enteredStatusCode = response.StatusCode;
            }
        }

        throw new YouTrackNotAnApiException(
            $"No YouTrack REST API answers at {enteredUrl}: {enteredApiUrl} answered "
                + $"{enteredAnswer}, and none of its parent paths answered as YouTrack either.",
            enteredApiUrl!,
            enteredStatusCode
        );
    }

    /// <summary>
    /// The second step of the connection check: the user the token belongs to.
    /// </summary>
    /// <exception cref="YouTrackUnauthorizedException">The token is wrong or missing.</exception>
    public async Task<YouTrackUserDto> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        return (
            await Get<YouTrackUserDto>($"api/users/me?fields={UserFields}", cancellationToken)
        )!;
    }

    /// <summary>
    /// The first issues a YouTrack query finds, in YouTrack's order.
    /// </summary>
    /// <exception cref="YouTrackInvalidQueryException">
    /// YouTrack can't parse the query, as happens while a query is half typed,
    /// e.g. "#Unres" or "state: Op".
    /// </exception>
    public async Task<ImmutableArray<YouTrackIssueDto>> SearchIssues(
        string query,
        int top = DefaultSearchTop,
        CancellationToken cancellationToken = default
    )
    {
        return await GetArray<YouTrackIssueDto>(
            $"api/issues?fields={IssueFields}&query={Uri.EscapeDataString(query)}&$top={top}",
            cancellationToken
        );
    }

    /// <summary>
    /// The issue with an ID such as "KC-230", in any case, or null when there is
    /// no such issue.
    /// </summary>
    public async Task<YouTrackIssueDto?> GetIssue(
        string id,
        CancellationToken cancellationToken = default
    )
    {
        return await Get<YouTrackIssueDto>(
            $"api/issues/{Uri.EscapeDataString(id.Trim())}?fields={IssueFields}",
            cancellationToken,
            notFoundAsNull: true
        );
    }

    /// <summary>The projects, archived ones left out.</summary>
    /// <exception cref="YouTrackUnavailableException">
    /// The token may not list projects.
    /// </exception>
    public async Task<ImmutableArray<YouTrackProjectDto>> GetProjects(
        CancellationToken cancellationToken = default
    )
    {
        var projects = await GetAll<YouTrackProjectDto>(
            "api/admin/projects",
            ProjectFields,
            cancellationToken
        );

        return projects
            .Where(p => !p.Archived)
            .Select(p => p with { IconUrl = ResolveUrl(p.IconUrl) })
            .ToImmutableArray();
    }

    /// <exception cref="YouTrackUnavailableException">
    /// The token may not list agile boards.
    /// </exception>
    public Task<ImmutableArray<YouTrackAgileBoardDto>> GetAgileBoards(
        CancellationToken cancellationToken = default
    ) => GetAll<YouTrackAgileBoardDto>("api/agiles", AgileBoardFields, cancellationToken);

    /// <exception cref="YouTrackUnavailableException">
    /// The token may not list saved searches.
    /// </exception>
    public Task<ImmutableArray<YouTrackSavedQueryDto>> GetSavedQueries(
        CancellationToken cancellationToken = default
    ) => GetAll<YouTrackSavedQueryDto>("api/savedQueries", SavedQueryFields, cancellationToken);

    /// <summary>
    /// The dashboards. The endpoint isn't in the REST API reference.
    /// </summary>
    /// <exception cref="YouTrackUnavailableException">
    /// The installation has no such endpoint, or the token may not use it.
    /// </exception>
    public Task<ImmutableArray<YouTrackDashboardDto>> GetDashboards(
        CancellationToken cancellationToken = default
    ) => GetAll<YouTrackDashboardDto>("api/dashboards", DashboardFields, cancellationToken);

    /// <summary>
    /// Users for the people directory: those the query finds, leaving out
    /// users without an email address, banned users and guests. The query
    /// parameter isn't in the REST API reference.
    /// </summary>
    /// <exception cref="YouTrackUnavailableException">
    /// The token may not list users (403).
    /// </exception>
    public async Task<ImmutableArray<YouTrackUserDto>> SearchUsers(
        string query,
        int top = DefaultSearchTop,
        CancellationToken cancellationToken = default
    )
    {
        var users = await GetArray<YouTrackUserDto>(
            $"api/users?fields={UserFields}&query={Uri.EscapeDataString(query)}&$top={top}",
            cancellationToken
        );

        return users
            .Where(p => !string.IsNullOrEmpty(p.Email) && !p.Banned && !p.Guest)
            .ToImmutableArray();
    }

    /// <summary>
    /// Downloads an icon or avatar. A host-relative URL is resolved against
    /// the service URL. The token goes only to the service URL's own scheme,
    /// host and port; a URL elsewhere, such as Gravatar or an external Hub, is
    /// fetched without it. Redirects don't carry it either: HttpClient drops
    /// the Authorization header when it follows one, and the built-in Hub
    /// answers a user without an uploaded avatar with a redirect to Gravatar.
    /// </summary>
    /// <exception cref="YouTrackUnauthorizedException">
    /// The service URL refused the token.
    /// </exception>
    /// <exception cref="YouTrackException">The download failed.</exception>
    public async Task<T> DownloadFile<T>(
        string url,
        Func<HttpContent, Task<T>> action,
        CancellationToken cancellationToken = default
    )
    {
        var absoluteUrl = YouTrackUrls.Resolve(ServiceUrl, url);
        var withToken = YouTrackUrls.IsSameOrigin(ServiceUrl, absoluteUrl);

        using var request = new HttpRequestMessage(HttpMethod.Get, absoluteUrl);

        if (withToken)
            request.Headers.Authorization = _authorization;

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (withToken)
        {
            _responseObserver?.Invoke(response.StatusCode);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new YouTrackUnauthorizedException(
                    $"{ServiceUrl} refused the token for {absoluteUrl}.",
                    absoluteUrl
                );
            }
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new YouTrackException(
                $"{absoluteUrl} answered {DescribeAnswer(response)}.",
                absoluteUrl,
                response.StatusCode
            );
        }

        return await action(response.Content);
    }

    private string? ResolveUrl(string? url) =>
        string.IsNullOrEmpty(url) ? url : YouTrackUrls.Resolve(ServiceUrl, url);

    private async Task<ImmutableArray<T>> GetAll<T>(
        string path,
        string fields,
        CancellationToken cancellationToken
    )
    {
        var results = ImmutableArray.CreateBuilder<T>();

        for (var skip = 0; ; skip += PageSize)
        {
            var page = await GetArray<T>(
                $"{path}?fields={fields}&$skip={skip}&$top={PageSize}",
                cancellationToken
            );

            results.AddRange(page);

            if (page.Length < PageSize)
                return results.ToImmutable();
        }
    }

    private async Task<ImmutableArray<T>> GetArray<T>(
        string pathAndQuery,
        CancellationToken cancellationToken
    )
    {
        var result = await Get<ImmutableArray<T>?>(pathAndQuery, cancellationToken);

        return result is { IsDefault: false } array ? array : ImmutableArray<T>.Empty;
    }

    private async Task<T?> Get<T>(
        string pathAndQuery,
        CancellationToken cancellationToken,
        bool notFoundAsNull = false
    )
    {
        var url = ServiceUrl + pathAndQuery;

        using var request = CreateRequest(url);

        request.Headers.Authorization = _authorization;

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        _responseObserver?.Invoke(response.StatusCode);

        var json = await ReadJson(response, cancellationToken);
        var error =
            json != null && !response.IsSuccessStatusCode
                ? TryDeserialize<YouTrackErrorDto>(json)
                : null;

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new YouTrackUnauthorizedException(
                $"{ServiceUrl} refused the token: {error?.ErrorDescription ?? "401 Unauthorized"}.",
                url
            );
        }

        if (json == null)
        {
            throw new YouTrackNotAnApiException(
                $"{url} answered {DescribeAnswer(response)} instead of JSON, so {ServiceUrl} "
                    + "isn't the address of a YouTrack REST API. Use the address YouTrack is "
                    + "opened on, e.g. https://youtrack.example.com/.",
                url,
                response.StatusCode
            );
        }

        if (response.StatusCode == HttpStatusCode.NotFound && notFoundAsNull)
            return default;

        if (!response.IsSuccessStatusCode)
        {
            var message =
                $"{url} answered {DescribeAnswer(response)}: "
                + (error?.ErrorDescription ?? error?.Error ?? "no details");

            throw response.StatusCode switch
            {
                HttpStatusCode.Forbidden
                or HttpStatusCode.NotFound
                    => new YouTrackUnavailableException(message, url, response.StatusCode),
                HttpStatusCode.BadRequest when error?.Error == "invalid_query"
                    => new YouTrackInvalidQueryException(message, url),
                _ => new YouTrackException(message, url, response.StatusCode)
            };
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException ex)
        {
            throw new YouTrackException(
                $"{url} answered JSON YouTrackClient can't read: {ex.Message}",
                url,
                response.StatusCode,
                ex
            );
        }
    }

    private static HttpRequestMessage CreateRequest(string url)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return request;
    }

    private static async Task<string?> ReadJson(
        HttpResponseMessage response,
        CancellationToken cancellationToken
    )
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType;

        var isJson =
            mediaType != null
            && (
                mediaType.Equals("application/json", StringComparison.OrdinalIgnoreCase)
                || mediaType.EndsWith("+json", StringComparison.OrdinalIgnoreCase)
            );

        return isJson ? await response.Content.ReadAsStringAsync(cancellationToken) : null;
    }

    private static T? TryDeserialize<T>(string json)
        where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string DescribeAnswer(HttpResponseMessage response)
    {
        var mediaType = response.Content.Headers.ContentType?.MediaType ?? "no content type";

        return $"HTTP {(int)response.StatusCode} with {mediaType}";
    }
}

/// <summary>
/// A YouTrack request failed. The base of the exceptions below, and itself
/// thrown for any other failure, such as a 500.
/// </summary>
internal class YouTrackException(
    string message,
    string requestUrl,
    HttpStatusCode? statusCode,
    Exception? innerException = null
) : HttpRequestException(message, innerException, statusCode)
{
    public string RequestUrl { get; } = requestUrl;
}

/// <summary>
/// 401: the token is wrong or missing. Measured: an invalid token and no token
/// both get 401.
/// </summary>
internal class YouTrackUnauthorizedException(string message, string requestUrl)
    : YouTrackException(message, requestUrl, HttpStatusCode.Unauthorized);

/// <summary>
/// 403 or 404 on a list endpoint: the installation lacks it or the token may
/// not use it. The plugin leaves the category out for the connection and logs
/// the error.
/// </summary>
internal class YouTrackUnavailableException(
    string message,
    string requestUrl,
    HttpStatusCode statusCode
) : YouTrackException(message, requestUrl, statusCode);

/// <summary>
/// 400 "invalid_query": YouTrack can't parse the search query.
/// </summary>
internal class YouTrackInvalidQueryException(string message, string requestUrl)
    : YouTrackException(message, requestUrl, HttpStatusCode.BadRequest);

/// <summary>
/// The answer isn't JSON, typically the web app's HTML page: the URL isn't the
/// address of a YouTrack REST API. <see cref="HttpRequestException.StatusCode"/>
/// is the status of that answer, often 200.
/// </summary>
internal class YouTrackNotAnApiException(
    string message,
    string requestUrl,
    HttpStatusCode? statusCode
) : YouTrackException(message, requestUrl, statusCode);
