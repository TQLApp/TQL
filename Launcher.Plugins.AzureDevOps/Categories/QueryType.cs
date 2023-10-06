using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class QueryType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.Query.Id;

    public QueryType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new QueryMatch(JsonSerializer.Deserialize<QueryMatchDto>(json)!, _images);
    }
}
