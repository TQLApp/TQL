using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoriesCategory : ICategory
{
    private readonly ICache<AzureData> _cache;

    public Guid Id { get; }
    public IImage Icon { get; }
    public string Title { get; }

    public RepositoriesCategory(IImageFactory imageFactory, ICache<AzureData> cache)
    {
        _cache = cache;

        Id = Guid.Parse("16b71d66-79e2-4b1e-bc33-813591408663");
        Icon = imageFactory.FromBytes(NeutralResources.Repositories);
        Title = "Azure Repository";
    }

    public Task<ImmutableArray<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
