using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class DashboardsMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly Images _images;
    private readonly string _url;

    public override string Text { get; }
    public override IImage Icon => _images.Dashboards;
    public override MatchTypeId TypeId => TypeIds.Dashboards;

    public DashboardsMatch(string text, Images images, string url, ICache<AzureData> cache)
        : base(cache)
    {
        _images = images;
        _url = url;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        return from project in data.GetConnection(_url).Projects
            from dashboard in project.Dashboards
            select new DashboardMatch(
                new DashboardMatchDto(_url, project.Name, dashboard.Id, dashboard.Name),
                _images
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
