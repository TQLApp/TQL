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
            select new UrlMatch(
                $"{project.Name}/{team.Name} {backlog.Name} Backlog",
                _images.Boards,
                $"{_url}/_backlogs/backlog/{Uri.EscapeDataString(team.Name)}/{Uri.EscapeDataString(backlog.Name)}"
            );
    }
}
