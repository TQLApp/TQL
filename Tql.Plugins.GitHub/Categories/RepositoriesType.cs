﻿using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class RepositoriesType : IMatchType
{
    private readonly ConnectionManager _connectionManager;
    private readonly GitHubApi _api;
    private readonly ICache<GitHubData> _cache;

    public Guid Id => TypeIds.Repositories.Id;

    public RepositoriesType(
        ConnectionManager connectionManager,
        GitHubApi api,
        ICache<GitHubData> cache
    )
    {
        _connectionManager = connectionManager;
        _api = api;
        _cache = cache;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Id == dto.Id))
            return null;

        return new RepositoriesMatch(
            MatchUtils.GetMatchLabel("GitHub Repository", _connectionManager, dto),
            dto,
            _api,
            _cache
        );
    }
}
