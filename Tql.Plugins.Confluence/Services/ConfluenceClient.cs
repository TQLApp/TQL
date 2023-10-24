using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Tql.Plugins.Confluence.Support;

namespace Tql.Plugins.Confluence.Services;

internal class ConfluenceClient
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationHeaderValue _authentication;
    private readonly string _baseUrl;

    public ConfluenceClient(HttpClient httpClient, Connection connection)
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

    public async Task<ImmutableArray<ConfluenceSpaceDto>> GetSpaces(
        int? limit = default,
        CancellationToken cancellationToken = default
    )
    {
        var results = ImmutableArray.CreateBuilder<ConfluenceSpaceDto>();
        var requestedLimit = limit;
        limit ??= 1000;

        for (var offset = 0; ; offset++)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_baseUrl}/rest/api/space?expand=icon&limit={limit}&start={offset * limit}"
            );

            var dto = await ExecuteJsonRequest<ConfluenceSpacesResponseDto>(
                request,
                cancellationToken
            );

            limit = dto.Limit;

            results.AddRange(dto.Dashboards);

            if (
                (requestedLimit.HasValue && results.Count >= requestedLimit.Value)
                || dto.Size <= limit * (offset + 1)
            )
                return results.ToImmutable();
        }
    }

    public async Task<T> DownloadFile<T>(string url, Func<HttpContent, Task<T>> action)
    {
        if (url.IndexOf("://", StringComparison.Ordinal) == -1)
            url = $"{_baseUrl}/{url.TrimStart('/')}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Authorization = _authentication;

        using var response = await _httpClient.SendAsync(request);

        response.EnsureSuccessStatusCode();

        return await action(response.Content);
    }

    private async Task<T> ExecuteJsonRequest<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.Authorization = _authentication;

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(json)!;
    }
}

internal record ConfluenceSpacesResponseDto(
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("results")] ImmutableArray<ConfluenceSpaceDto> Dashboards
);

internal record ConfluenceSpaceDto(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("icon")] ConfluenceSpaceIconDto Icon,
    [property: JsonPropertyName("_links")] ConfluenceSpaceLinksDto Links
);

internal record ConfluenceSpaceIconDto([property: JsonPropertyName("path")] string Path);

internal record ConfluenceSpaceLinksDto([property: JsonPropertyName("webui")] string WebUI);
