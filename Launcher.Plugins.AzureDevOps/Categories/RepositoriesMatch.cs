using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoriesMatch : ISearchableMatch
{
    private readonly Images _images;
    private readonly string _url;

    public string Text { get; }
    public IImage Icon => _images.Repositories;

    public RepositoriesMatch(string text, Images images, string url)
    {
        _images = images;
        _url = url;

        Text = text;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var cache = await context.ServiceProvider.GetRequiredService<ICache<AzureData>>().Get();

        return context.Filter(
            from project in cache.GetConnection(_url).Projects
            from repository in project.Repositories
            select new RepositoryMatch(
                _images,
                new RepositoryDto(_url, project.Name, repository.Name)
            ),
            text
        );
    }
}
