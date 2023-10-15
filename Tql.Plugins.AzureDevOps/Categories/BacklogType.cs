using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BacklogType : IMatchType
{
    public Guid Id => TypeIds.Backlog.Id;

    public IMatch Deserialize(string json)
    {
        return new BacklogMatch(JsonSerializer.Deserialize<BacklogMatchDto>(json)!);
    }
}
