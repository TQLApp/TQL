﻿using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class QueriesType : IMatchType
{
    private readonly ICache<AzureData> _cache;
    private readonly ConnectionManager _connectionManager;
    private readonly AzureDevOpsApi _api;
    private readonly AzureWorkItemIconManager _iconManager;

    public Guid Id => TypeIds.Queries.Id;

    public QueriesType(
        ICache<AzureData> cache,
        ConnectionManager connectionManager,
        AzureDevOpsApi api,
        AzureWorkItemIconManager iconManager
    )
    {
        _cache = cache;
        _connectionManager = connectionManager;
        _api = api;
        _iconManager = iconManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        if (!_connectionManager.Connections.Any(p => p.Url == dto.Url))
            return null;

        return new QueriesMatch(
            MatchUtils.GetMatchLabel("Azure Query", _connectionManager, dto.Url),
            dto.Url,
            _cache,
            _api,
            _iconManager
        );
    }
}