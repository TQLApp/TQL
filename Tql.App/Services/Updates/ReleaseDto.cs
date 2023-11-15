using System.Text.Json.Serialization;

namespace Tql.App.Services.Updates;

internal record ReleaseDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("tag_name")] string TagName,
    [property: JsonPropertyName("prerelease")] bool Prerelease,
    [property: JsonPropertyName("created_at")] DateTime CreatedAt,
    [property: JsonPropertyName("assets")] ImmutableArray<ReleaseAssetDto> Assets
);

internal record ReleaseAssetDto(
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("url")] string Url
);
