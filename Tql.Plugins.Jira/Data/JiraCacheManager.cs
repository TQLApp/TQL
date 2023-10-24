using SharpVectors.Net;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;

namespace Tql.Plugins.Jira.Data;

internal class JiraCacheManager : ICacheManager<JiraData>
{
    private readonly ConnectionManager _connectionManager;
    private readonly HttpClient _httpClient;

    public int Version => 1;

    public JiraCacheManager(ConnectionManager connectionManager, HttpClient httpClient)
    {
        _connectionManager = connectionManager;
        _httpClient = httpClient;
    }

    public async Task<JiraData> Create()
    {
        var results = ImmutableArray.CreateBuilder<JiraConnection>();

        foreach (var connection in _connectionManager.Connections)
        {
            results.Add(await CreateConnection(connection));
        }

        return new JiraData(results.ToImmutable());
    }

    private async Task<JiraConnection> CreateConnection(Connection connection)
    {
        var client = connection.CreateClient();

        var dashboards = await GetDashboards(connection);

        return new JiraConnection(connection.Url, dashboards);
    }

    private async Task<ImmutableArray<JiraDashboard>> GetDashboards(Connection connection)
    {
        var results = ImmutableArray.CreateBuilder<JiraDashboard>();
        var maxResults = 1000;

        for (var offset = 0; ; offset++)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{connection.Url.TrimEnd('/')}/rest/api/2/dashboard?maxResults={maxResults}&startAt={offset * maxResults}"
            );

            request.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                Encryption.Unprotect(connection.ProtectedPatToken)
            );

            using var response = await _httpClient.SendAsync(request);

            var json = await response.Content.ReadAsStringAsync();

            var dto = JsonSerializer.Deserialize<JiraDashboardsDto>(json)!;

            maxResults = dto.MaxResults;

            results.AddRange(dto.Dashboards.Select(p => new JiraDashboard(p.Id, p.Name, p.View)));

            if (dto.Total <= maxResults * (offset + 1))
                return results.ToImmutable();
        }
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
