using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryType : IMatchType
{
    public Guid Id => TypeIds.Repository.Id;

    public IMatch Deserialize(string json)
    {
        return new RepositoryMatch(JsonSerializer.Deserialize<RepositoryMatchDto>(json)!);
    }
}
