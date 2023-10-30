using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Services;

internal class ConfluenceClient
{
    private readonly HttpClient _httpClient;
    private readonly Connection _connection;
    private readonly IUI _ui;
    private readonly AuthenticationHeaderValue _authentication;
    private readonly string _baseUrl;

    public ConfluenceClient(HttpClient httpClient, Connection connection, IUI ui)
    {
        _httpClient = httpClient;
        _connection = connection;
        _ui = ui;

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

    public Task<ImmutableArray<ConfluenceSpaceDto>> GetSpaces(
        int? limit = default,
        CancellationToken cancellationToken = default
    ) =>
        GetPagedQuery<ConfluenceSpaceDto>(
            $"{_baseUrl}/rest/api/space?expand=icon",
            limit,
            cancellationToken
        );

    public Task<ImmutableArray<ConfluenceSiteSearchDto>> SiteSearch(
        string cql,
        int? limit = default,
        CancellationToken cancellationToken = default
    ) =>
        GetPagedQuery<ConfluenceSiteSearchDto>(
            $"{_baseUrl}/rest/api/search?cql={Uri.EscapeDataString(cql)}&includeArchivedSpaces=false&excerpt=none",
            limit,
            cancellationToken
        );

    private async Task<ImmutableArray<T>> GetPagedQuery<T>(
        string url,
        int? limit = default,
        CancellationToken cancellationToken = default
    )
    {
        var results = ImmutableArray.CreateBuilder<T>();
        var requestedLimit = limit;
        limit ??= 1000;

        for (var offset = 0; ; offset++)
        {
            var thisUrl =
                url + (url.Contains('?') ? "&" : "?") + $"limit={limit}&start={offset * limit}";

            using var request = new HttpRequestMessage(HttpMethod.Get, thisUrl);

            var dto = await ExecuteJsonRequest<ConfluencePageBeanDto<T>>(
                request,
                cancellationToken
            );

            limit = dto.Limit;

            results.AddRange(dto.Results);

            if (
                (requestedLimit.HasValue && results.Count >= requestedLimit.Value)
                || dto.Size <= limit * (offset + 1)
            )
                return results.ToImmutable();
        }
    }

    public async Task<T> DownloadFile<T>(string url, Func<HttpContent, Task<T>> action)
    {
        if (!url.Contains("://", StringComparison.Ordinal))
            url = $"{_baseUrl}/{url.TrimStart('/')}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        request.Headers.Authorization = _authentication;

        using var response = await _httpClient.SendAsync(request);

        ShowUnauthorizedNotification(response);

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

        ShowUnauthorizedNotification(response);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<T>(json)!;
    }

    private void ShowUnauthorizedNotification(HttpResponseMessage response)
    {
        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _ui.ShowNotificationBar(
                $"{ConfluencePlugin.Id}/ConnectionFailed/{_connection.Id}",
                $"Unable to connect to Confluence - {_connection.Name}. Click here to open the "
                    + $"Confluence settings screen and validate your credentials.",
                () => _ui.OpenConfiguration(ConfluencePlugin.ConfigurationPageId)
            );
        }
        else
        {
            _ui.RemoveNotificationBar($"{ConfluencePlugin.Id}/ConnectionFailed/{_connection.Id}");
        }
    }
}

internal record ConfluencePageBeanDto<T>(
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("limit")] int Limit,
    [property: JsonPropertyName("size")] int Size,
    [property: JsonPropertyName("results")] ImmutableArray<T> Results
);

internal record ConfluenceSpaceDto(
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("icon")] ConfluenceSpaceIconDto Icon,
    [property: JsonPropertyName("_links")] ConfluenceSpaceLinksDto Links
);

internal record ConfluenceSpaceIconDto([property: JsonPropertyName("path")] string Path);

internal record ConfluenceSpaceLinksDto([property: JsonPropertyName("webui")] string WebUI);

internal record ConfluenceSiteSearchDto(
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("url")] string Url,
    [property: JsonPropertyName("resultGlobalContainer")]
        ConfluenceSiteSearchGlobalContainerDto? ResultGlobalContainer,
    [property: JsonPropertyName("content")] ConfluenceSiteSearchContentDto? Content
);

internal record ConfluenceSiteSearchGlobalContainerDto(
    [property: JsonPropertyName("title")] string Title
);

internal record ConfluenceSiteSearchContentDto(
    [property: JsonPropertyName("_links")] ConfluenceSiteSearchContentLinksDto Links
);

internal record ConfluenceSiteSearchContentLinksDto(
    [property: JsonPropertyName("tinyui")] string TinyUI
);
