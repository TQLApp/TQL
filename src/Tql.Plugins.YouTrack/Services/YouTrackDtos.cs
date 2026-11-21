using System.Text.Json.Serialization;

namespace Tql.Plugins.YouTrack.Services;

// The JSON shapes of the YouTrack REST API, limited to the fields YouTrackClient
// requests. YouTrack adds a "$type" to every object; only the configuration
// check reads it. URL fields hold absolute URLs: YouTrackClient resolves the
// host-relative ones the API returns.
//
// This file has no WPF or TQL dependencies, so the plain net8.0 test project
// can compile it.

internal record YouTrackConfigDto(
    [property: JsonPropertyName("$type")] string? Type,
    [property: JsonPropertyName("version")] string? Version
);

internal record YouTrackErrorDto(
    [property: JsonPropertyName("error")] string? Error,
    [property: JsonPropertyName("error_description")] string? ErrorDescription
);

internal record YouTrackUserDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("login")] string Login,
    [property: JsonPropertyName("fullName")] string? FullName,
    [property: JsonPropertyName("email")] string? Email,
    [property: JsonPropertyName("banned")] bool Banned,
    [property: JsonPropertyName("guest")] bool Guest
);

internal record YouTrackIssueDto(
    [property: JsonPropertyName("idReadable")] string IdReadable,
    [property: JsonPropertyName("summary")] string Summary
);

internal record YouTrackProjectDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("shortName")] string ShortName,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("archived")] bool Archived,
    [property: JsonPropertyName("iconUrl")] string? IconUrl
);

internal record YouTrackAgileBoardDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);

internal record YouTrackSavedQueryDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("query")] string Query
);

internal record YouTrackDashboardDto(
    [property: JsonPropertyName("id")] string Id,
    [property: JsonPropertyName("name")] string Name
);

/// <summary>
/// The outcome of <see cref="YouTrackClient.CheckServiceUrl"/>.
/// </summary>
/// <param name="EnteredUrl">The URL as entered, normalized.</param>
/// <param name="ServiceUrl">
/// The URL the REST API answered on: <paramref name="EnteredUrl"/> or one of
/// its parent paths.
/// </param>
/// <param name="Version">The YouTrack version, e.g. "2026.2".</param>
internal record YouTrackServiceCheck(string EnteredUrl, string ServiceUrl, string? Version)
{
    /// <summary>
    /// Whether the REST API answered on a parent path rather than on the
    /// entered URL, so the dialog should offer <see cref="ServiceUrl"/>.
    /// </summary>
    public bool IsSuggestion => EnteredUrl != ServiceUrl;
}
