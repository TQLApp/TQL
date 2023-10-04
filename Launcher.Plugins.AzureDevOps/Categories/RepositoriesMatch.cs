using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class RepositoriesMatch : CachedMatch<AzureData>
{
    private readonly Images _images;
    private readonly string _url;

    public override string Text { get; }
    public override IImage Icon => _images.Repositories;
    public override MatchTypeId TypeId { get; } =
        new(Guid.Parse("b2b479f8-572e-4a02-a87e-c1ba244b17cf"), AzureDevOpsPlugin.Id);

    public RepositoriesMatch(string text, Images images, string url, ICache<AzureData> cache)
        : base(cache)
    {
        _images = images;
        _url = url;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(_url).Projects
            from repository in project.Repositories
            select new RepositoryMatch(
                _images,
                new RepositoryMatchDto(_url, project.Name, repository.Name)
            );
    }
}
