using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoryType : IMatchType
{
    public Guid Id => TypeIds.Repository.Id;

    public IMatch Deserialize(string json)
    {
        return new RepositoryMatch(JsonSerializer.Deserialize<RepositoryMatchDto>(json)!);
    }
}
