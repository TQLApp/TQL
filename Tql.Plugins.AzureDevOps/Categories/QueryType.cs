using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class QueryType : IMatchType
{
    private readonly AzureDevOpsApi _api;
    private readonly AzureWorkItemIconManager _iconManager;
    public Guid Id => TypeIds.Query.Id;

    public QueryType(AzureDevOpsApi api, AzureWorkItemIconManager iconManager)
    {
        _api = api;
        _iconManager = iconManager;
    }

    public IMatch Deserialize(string json)
    {
        return new QueryMatch(JsonSerializer.Deserialize<QueryMatchDto>(json)!, _api, _iconManager);
    }
}
