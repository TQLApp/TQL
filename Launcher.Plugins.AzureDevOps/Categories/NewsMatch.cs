using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Utilities;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class NewsMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly Images _images;
    private readonly string _url;

    public override string Text { get; }
    public override IImage Icon => _images.Azure;
    public override MatchTypeId TypeId => TypeIds.News;

    public NewsMatch(string text, Images images, string url, ICache<AzureData> cache)
        : base(cache)
    {
        _images = images;
        _url = url;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        foreach (var project in data.GetConnection(_url).Projects)
        {
            yield return new NewMatch(
                new NewMatchDto(_url, project.Name, NewMatchType.Query, null),
                _images
            );

            foreach (var workItemType in project.WorkItemTypes)
            {
                yield return new NewMatch(
                    new NewMatchDto(_url, project.Name, NewMatchType.WorkItem, workItemType.Name),
                    _images
                );
            }
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
