using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathType : IMatchType
{
    public static Guid Id = Guid.Parse("07461529-2c9a-4bda-be99-a94b8640fe7d");

    private readonly Images _images;

    Guid IMatchType.Id => Id;

    public RepositoryFilePathType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string text, string json)
    {
        return new RepositoryFilePathMatch(
            JsonSerializer.Deserialize<RepositoryFilePathMatchDto>(json)!,
            _images
        );
    }
}
