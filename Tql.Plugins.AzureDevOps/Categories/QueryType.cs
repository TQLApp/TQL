using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class QueryType : IMatchType
{
    private readonly AzureDevOpsApi _api;
    private readonly AzureWorkItemIconManager _iconManager;
    private readonly ConfigurationManager _configurationManager;
    public Guid Id => TypeIds.Query.Id;

    public QueryType(
        AzureDevOpsApi api,
        AzureWorkItemIconManager iconManager,
        ConfigurationManager configurationManager
    )
    {
        _api = api;
        _iconManager = iconManager;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<QueryMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.Url))
            return null;

        return new QueryMatch(dto, _api, _iconManager);
    }
}
