using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoryFilePathType : IMatchType
{
    public static MatchTypeId TypeId =
        new(Guid.Parse("07461529-2c9a-4bda-be99-a94b8640fe7d"), AzureDevOpsPlugin.Id);

    private readonly Images _images;

    public Guid Id => TypeId.Id;

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
