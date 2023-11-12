using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BacklogType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;
    private readonly ICache<AzureData> _cache;
    private readonly AzureDevOpsApi _api;
    private readonly AzureWorkItemIconManager _iconManager;

    public Guid Id => TypeIds.Backlog.Id;

    public BacklogType(
        ConfigurationManager configurationManager,
        ICache<AzureData> cache,
        AzureDevOpsApi api,
        AzureWorkItemIconManager iconManager
    )
    {
        _configurationManager = configurationManager;
        _cache = cache;
        _api = api;
        _iconManager = iconManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<BacklogMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new BacklogMatch(dto, _cache, _api, _iconManager);
    }
}
