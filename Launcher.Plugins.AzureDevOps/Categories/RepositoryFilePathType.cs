using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathType : IMatchType
{
    private readonly Images _images;

    public Guid Id => TypeIds.RepositoryFilePath.Id;

    public RepositoryFilePathType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new RepositoryFilePathMatch(
            JsonSerializer.Deserialize<RepositoryFilePathMatchDto>(json)!,
            _images
        );
    }
}
