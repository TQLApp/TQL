using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class QueryType : IMatchType
{
    public Guid Id => TypeIds.Query.Id;

    public IMatch Deserialize(string json)
    {
        return new QueryMatch(JsonSerializer.Deserialize<QueryMatchDto>(json)!);
    }
}
