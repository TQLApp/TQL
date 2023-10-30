using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class NewsMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly string _url;
    private readonly IconCacheManager _iconCacheManager;

    public override string Text { get; }
    public override ImageSource Icon => Images.Issues;
    public override MatchTypeId TypeId => TypeIds.News;

    public NewsMatch(
        string text,
        string url,
        ICache<JiraData> cache,
        IconCacheManager iconCacheManager
    )
        : base(cache)
    {
        _url = url;
        _iconCacheManager = iconCacheManager;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        return from project in data.GetConnection(_url).Projects
            from issueType in project.IssueTypes
            select new NewMatch(
                new NewMatchDto(
                    _url,
                    project.Name,
                    project.Id,
                    issueType.Name,
                    issueType.Id,
                    issueType.IconUrl
                ),
                _iconCacheManager
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
