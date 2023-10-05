using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoryType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.Repository.Id;

    public RepositoryType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new RepositoryMatch(_images, JsonSerializer.Deserialize<RepositoryMatchDto>(json)!);
    }
}
