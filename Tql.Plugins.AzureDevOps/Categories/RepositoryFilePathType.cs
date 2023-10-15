using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathType : IMatchType
{
    public Guid Id => TypeIds.RepositoryFilePath.Id;

    public IMatch Deserialize(string json)
    {
        return new RepositoryFilePathMatch(
            JsonSerializer.Deserialize<RepositoryFilePathMatchDto>(json)!
        );
    }
}
