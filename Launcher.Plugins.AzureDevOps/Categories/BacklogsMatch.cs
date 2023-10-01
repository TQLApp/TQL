using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class BacklogsMatch : CachedMatch<AzureData>
{
    private readonly Images _images;
    private readonly string _url;

    public override string Text { get; }
    public override IImage Icon => _images.Repositories;
    public override Guid TypeId => Guid.Parse("14365335-45a2-43a3-af7d-fe2229744cc7");

    public BacklogsMatch(string text, Images images, string url, ICache<AzureData> cache)
        : base(cache)
    {
        _images = images;
        _url = url;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(_url).Projects
            from team in project.Teams
            from backlog in project.Backlogs
            select new BacklogMatch(
                new BacklogMatchDto(_url, project.Name, team.Name, backlog.Name),
                _images
            );
    }
}
