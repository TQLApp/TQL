﻿using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Tql.Abstractions;

namespace Tql.Plugins.Jira.Services;

internal class JiraClient
{
    private readonly HttpClient _httpClient;
    private readonly Connection _connection;
    private readonly IUI _ui;
    private readonly AuthenticationHeaderValue _authentication;
    private readonly string _baseUrl;

    public JiraClient(HttpClient httpClient, Connection connection, IUI ui, IEncryption encryption)
    {
        _httpClient = httpClient;
        _connection = connection;
        _ui = ui;

        _baseUrl = connection.Url.TrimEnd('/');

        var password = encryption.DecryptString(connection.ProtectedPassword);

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

            var dto = await ExecuteJsonRequest<JiraDashboardsResponseDto>(
                request,
                cancellationToken
            );

            maxResults = dto.MaxResults;

            results.AddRange(dto.Dashboards);

            if (
                (requestedMaxResults.HasValue && results.Count >= requestedMaxResults.Value)
                || dto.Total <= maxResults * (offset + 1)
            )
                return results.ToImmutable();
        }
    }

    public async Task<ImmutableArray<JiraProjectDto>> GetProjects(
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}/rest/api/2/project?expand=issueTypes"
        );

        return await ExecuteJsonRequest<ImmutableArray<JiraProjectDto>>(request, cancellationToken);
    }

    public Task<ImmutableArray<JiraBoardV3Dto>> GetAgileBoards(
        int? maxResults = null,
        CancellationToken cancellationToken = default
    ) =>
        GetPagedV3<JiraBoardV3Dto>(
            $"{_baseUrl}/rest/agile/1.0/board",
            maxResults,
            cancellationToken
        );

    public async Task<JiraBoardConfigurationV3Dto> GetAgileBoardsConfiguration(
        int boardId,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}/rest/agile/1.0/board/{boardId}/configuration"
        );

        return await ExecuteJsonRequest<JiraBoardConfigurationV3Dto>(request, cancellationToken);
    }

    private async Task<ImmutableArray<T>> GetPagedV3<T>(
        string url,
        int? maxResults,
        CancellationToken cancellationToken
    )
    {
        var results = ImmutableArray.CreateBuilder<T>();
        var requestedMaxResults = maxResults;
        maxResults ??= 1000;

        for (var offset = 0; ; offset++)
        {
            var thisUrl =
                url
                + (url.Contains('?') ? "&" : "?")
                + $"maxResults={maxResults}&startAt={offset * maxResults}";

            using var request = new HttpRequestMessage(HttpMethod.Get, thisUrl);

            var dto = await ExecuteJsonRequest<JiraPageBeanV3Dto<T>>(request, cancellationToken);

            maxResults = dto.MaxResults;

            results.AddRange(dto.Values);

            if (
                (requestedMaxResults.HasValue && results.Count >= requestedMaxResults.Value)
                || dto.Total <= maxResults * (offset + 1)
            )
                return results.ToImmutable();
        }
    }

    public async Task<ImmutableArray<JiraIssueDto>> SearchIssues(
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

            var dto = await ExecuteJsonRequest<JiraIssuesResponseDto>(request, cancellationToken);

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

        ShowUnauthorizedNotification(response);

        response.EnsureSuccessStatusCode();

        return await action(response.Content);
    }

    public async Task<JiraIssueDto> GetIssue(
        string key,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}/rest/api/2/issue/{Uri.EscapeDataString(key)}"
        );

        return await ExecuteJsonRequest<JiraIssueDto>(request, cancellationToken);
    }

    public async Task<ImmutableArray<JiraUserDto>> SearchUsers(
        string search,
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}/rest/api/2/user/search?username={Uri.EscapeDataString(search)}"
        );

        return await ExecuteJsonRequest<ImmutableArray<JiraUserDto>>(request, cancellationToken);
    }

    public async Task<ImmutableArray<JiraUserV3Dto>> SearchUsersV3(
        string search,
        int? maxResults = null,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{_baseUrl}/rest/api/3/user/search?query={Uri.EscapeDataString(search)}";
        if (maxResults.HasValue)
            url += $"&maxResults={maxResults}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        return await ExecuteJsonRequest<ImmutableArray<JiraUserV3Dto>>(request, cancellationToken);
    }

    public async Task<JiraXBoardConfigDto> GetXBoardConfig(
        int boardId,
        CancellationToken cancellationToken = default
    )
    {
        var url = $"{_baseUrl}/rest/greenhopper/1.0/xboard/config?rapidViewId={boardId}";

        using var request = new HttpRequestMessage(HttpMethod.Get, url);

        return await ExecuteJsonRequest<JiraXBoardConfigDto>(request, cancellationToken);
    }

    public async Task<ImmutableArray<JiraFilterDto>> GetFavoriteFilters(
        CancellationToken cancellationToken = default
    )
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_baseUrl}/rest/api/2/filter/favourite"
        );

        return await ExecuteJsonRequest<ImmutableArray<JiraFilterDto>>(request, cancellationToken);
    }

    public Task<ImmutableArray<JiraFilterDto>> GetFiltersV3(
        int? maxResults = null,
        CancellationToken cancellationToken = default
    ) =>
        GetPagedV3<JiraFilterDto>(
            $"{_baseUrl}/rest/api/3/filter/search?expand=jql,viewUrl",
            maxResults,
            cancellationToken
        );

    private async Task<T> ExecuteJsonRequest<T>(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Headers.Authorization = _authentication;

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        ShowUnauthorizedNotification(response);

        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);

        return JsonSerializer.Deserialize<T>(json)!;
    }

    private void ShowUnauthorizedNotification(HttpResponseMessage response)
    {
        var notificationKey = $"{JiraPlugin.Id}/ConnectionFailed/{_connection.Id}";

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            _ui.ShowNotificationBar(
                notificationKey,
                string.Format(
                    Labels.JiraClient_UnableToConnect,
                    string.Format(Labels.JiraClient_ResourceName, _connection.Name),
                    Labels.JiraPlugin_Title
                ),
                _ => _ui.OpenConfiguration(JiraPlugin.ConfigurationPageId)
            );
        }
        else
        {
            _ui.RemoveNotificationBar(notificationKey);
        }
    }
}

internal record JiraDashboardsResponseDto(
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

internal record JiraIssuesResponseDto(
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

internal record JiraProjectDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("key")] string Key,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("avatarUrls")] ImmutableDictionary<string, string> AvatarUrls,
    [property: JsonPropertyName("projectTypeKey")] string ProjectTypeKey,
    [property: JsonPropertyName("style")] string? Style,
    [property: JsonPropertyName("simplified")] bool Simplified,
    [property: JsonPropertyName("issueTypes")] ImmutableArray<JiraProjectIssueTypeDto>? IssueTypes
);

internal record JiraProjectIssueTypeDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("iconUrl")] string IconUrl,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("subTask")] bool SubTask,
    [property: JsonPropertyName("avatarId")] int AvatarId,
    [property: JsonPropertyName("hierarchyLevel")] int HierarchyLevel
);

internal record JiraPageBeanV3Dto<T>(
    [property: JsonPropertyName("isLast")] bool IsLast,
    [property: JsonPropertyName("maxResults")] int MaxResults,
    [property: JsonPropertyName("total")] int Total,
    [property: JsonPropertyName("values")] ImmutableArray<T> Values
);

internal record JiraBoardV3Dto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("type")] string Type,
    [property: JsonPropertyName("location")] JiraBoardLocationV3Dto? Location
);

internal record JiraBoardLocationV3Dto(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("projectName")] string ProjectName,
    [property: JsonPropertyName("projectKey")] string ProjectKey,
    [property: JsonPropertyName("projectTypeKey")] string ProjectTypeKey,
    [property: JsonPropertyName("avatarURI")] string AvatarUri,
    [property: JsonPropertyName("name")] string Name
);

internal record JiraUserDto(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("emailAddress")] string EmailAddress
);

internal record JiraUserV3Dto(
    [property: JsonPropertyName("displayName")] string DisplayName,
    [property: JsonPropertyName("emailAddress")] string EmailAddress
);

internal record JiraXBoardConfigDto(
    [property: JsonPropertyName("currentViewConfig")]
        JiraXBoardCurrentViewConfigDto CurrentViewConfig
);

internal record JiraXBoardCurrentViewConfigDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("quickFilters")]
        ImmutableArray<JiraXBoardQuickFilterDto> QuickFilters,
    [property: JsonPropertyName("isIssueListBacklog")] bool IsIssueListBacklog,
    [property: JsonPropertyName("sprintSupportEnabled")] bool SprintSupportEnabled
);

internal record JiraXBoardQuickFilterDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("query")] string Query,
    [property: JsonPropertyName("description")] string Description
);

internal record JiraBoardConfigurationV3Dto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("filter")] JiraBoardConfigurationFilterV3Dto Filter
);

internal record JiraBoardConfigurationFilterV3Dto([property: JsonPropertyName("id")] string Id);

internal record JiraFilterDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("jql")] string Jql,
    [property: JsonPropertyName("viewUrl")] string ViewUrl
);
