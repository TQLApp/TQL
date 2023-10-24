using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Tql.Plugins.Jira.Support;

namespace Tql.Plugins.Jira.Services;

internal class JiraClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationHeaderValue _authentication;
    private readonly string _baseUrl;

    public JiraClient(HttpClient httpClient, Connection connection)
    {
        _httpClient = httpClient;

        _baseUrl = connection.Url.TrimEnd('/');

        var password = Encryption.Unprotect(connection.ProtectedPassword);

        if (string.IsNullOrEmpty(connection.UserName))
        {
            _authentication = new AuthenticationHeaderValue("Bearer", password);
        }
        else
        {
            var parameter = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{connection.UserName}:{password}")
            );

            _authentication = new AuthenticationHeaderValue("Basic", parameter);
        }
    }

    public async Task<ImmutableArray<JiraDashboardDto>> GetDashboards(
        int? maxResults = default,
        CancellationToken cancellationToken = default
    )
    {
        var results = ImmutableArray.CreateBuilder<JiraDashboardDto>();
        var requestedMaxResults = maxResults;
        maxResults ??= 1000;

        for (var offset = 0; ; offset++)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_baseUrl}/rest/api/2/dashboard?maxResults={maxResults}&startAt={offset * maxResults}"
            );

            request.Headers.Authorization = _authentication;

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<JiraDashboardsDto>(json)!;

            maxResults = dto.MaxResults;

            results.AddRange(dto.Dashboards);

            if (
                (requestedMaxResults.HasValue && results.Count >= requestedMaxResults.Value)
                || dto.Total <= maxResults * (offset + 1)
            )
                return results.ToImmutable();
        }
    }

    public async Task<ImmutableArray<JiraIssueDto>> GetIssues(
        string jql,
        int? maxResults = default,
        CancellationToken cancellationToken = default
    )
    {
        var results = ImmutableArray.CreateBuilder<JiraIssueDto>();
        var requestedMaxResults = maxResults;
        maxResults ??= 1000;

        for (var offset = 0; ; offset++)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                $"{_baseUrl}/rest/api/2/search"
            );

            request.Headers.Authorization = _authentication;

            request.Content = new StringContent(
                JsonSerializer.Serialize(
                    new JiraIssuesRequestDto(
                        jql,
                        offset * maxResults.Value,
                        maxResults.Value,
                        ImmutableArray.Create("*all")
                    )
                ),
                Encoding.UTF8,
                "application/json"
            );

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<JiraIssuesDto>(json)!;

            maxResults = dto.MaxResults;

            results.AddRange(dto.Issues);

            if (
                (requestedMaxResults.HasValue && results.Count >= requestedMaxResults.Value)
                || dto.Total <= maxResults * (offset + 1)
            )
                return results.ToImmutable();
        }
    }

    public async Task<T> DownloadFile<T>(string url, Func<HttpContent, Task<T>> action)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Authorization = _authentication;

        using var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await action(response.Content);
    }
}

internal record JiraDashboardsDto(
    [property: JsonPropertyName("startAt")] int StartAt,
    [property: JsonPropertyName("maxResults")] int MaxResults,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("dashboards")] ImmutableArray<JiraDashboardDto> Dashboards
);

internal record JiraDashboardDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("self")] string Self,
    [property: JsonPropertyName("view")] string View
);

internal record JiraIssuesRequestDto(
    [property: JsonPropertyName("jql")] string Jql,
    [property: JsonPropertyName("startAt")] int StartAt,
    [property: JsonPropertyName("maxResults")] int MaxResults,
    [property: JsonPropertyName("fields")] ImmutableArray<string> Fields
);

internal record JiraIssuesDto(
    [property: JsonPropertyName("startAt")] int StartAt,
    [property: JsonPropertyName("maxResults")] int MaxResults,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("issues")] ImmutableArray<JiraIssueDto> Issues
);

internal record JiraIssueDto(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("fields")] JiraIssueFieldsDto Fields
);

internal record JiraIssueFieldsDto(
    [property: JsonPropertyName("summary")] string Summary,
    [property: JsonPropertyName("issuetype")] JiraIssueTypeDto IssueType
);

internal record JiraIssueTypeDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("iconUrl")] string IconUrl
);
