using System.Net.Http;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class GistsType : IMatchType
{
    private readonly ConnectionManager _connectionManager;
    private readonly GitHubApi _api;
    private readonly ICache<GitHubData> _cache;
    private readonly HttpClient _httpClient;

    public Guid Id => TypeIds.Gists.Id;

    public GistsType(
        ConnectionManager connectionManager,
        GitHubApi api,
        ICache<GitHubData> cache,
        HttpClient httpClient
    )
    {
        _connectionManager = connectionManager;
        _api = api;
        _cache = cache;
        _httpClient = httpClient;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Id == dto.Id))
            return null;

        return new GistsMatch(
            MatchUtils.GetMatchLabel("GitHub Gist", _connectionManager, dto),
            dto,
            _api,
            _cache,
            _httpClient
        );
    }
}
