using Launcher.Abstractions;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoryType : IMatchType
{
    private readonly Images _images;

    public static readonly Guid Id = Guid.Parse("c43afa23-6d71-4940-ba4c-256d1a5d13bb");

    Guid IMatchType.Id => Id;

    public RepositoryType(Images images)
    {
        _images = images;
    }

    public IMatch Deserialize(string json)
    {
        return new RepositoryMatch(_images, JsonSerializer.Deserialize<RepositoryMatchDto>(json)!);
    }
}
