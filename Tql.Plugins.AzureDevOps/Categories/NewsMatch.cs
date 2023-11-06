using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class NewsMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly string _url;
    private readonly AzureWorkItemIconManager _iconManager;

    public override string Text { get; }
    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.News;

    public NewsMatch(
        string text,
        string url,
        ICache<AzureData> cache,
        AzureWorkItemIconManager iconManager
    )
        : base(cache)
    {
        _url = url;
        _iconManager = iconManager;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        foreach (var project in data.GetConnection(_url).Projects)
        {
            yield return new NewMatch(
                new NewMatchDto(_url, project.Name, NewMatchType.Query, null),
                _iconManager
            );

            foreach (var workItemType in project.WorkItemTypes)
            {
                yield return new NewMatch(
                    new NewMatchDto(_url, project.Name, NewMatchType.WorkItem, workItemType.Name),
                    _iconManager
                );
            }
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
