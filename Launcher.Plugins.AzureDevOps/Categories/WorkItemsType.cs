﻿using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Plugins.AzureDevOps.Support;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

[RootMatchType]
internal class WorkItemsType : IMatchType
{
    private readonly Images _images;
    private readonly ICache<AzureData> _cache;
    private readonly ConnectionManager _connectionManager;
    private readonly AzureDevOpsApi _api;

    public Guid Id => TypeIds.WorkItems.Id;

    public WorkItemsType(
        Images images,
        ICache<AzureData> cache,
        ConnectionManager connectionManager,
        AzureDevOpsApi api
    )
    {
        _images = images;
        _cache = cache;
        _connectionManager = connectionManager;
        _api = api;
    }

    public IMatch Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;

        return new WorkItemsMatch(
            MatchUtils.GetMatchLabel("Azure Work Item", _connectionManager, dto.Url),
            _images,
            dto.Url,
            _cache,
            _api
        );
    }
}