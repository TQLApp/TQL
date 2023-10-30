using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class NewsMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly string _url;
    private readonly ICache<JiraData> _cache;
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
        _cache = cache;
        _iconCacheManager = iconCacheManager;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        foreach (var project in data.GetConnection(_url).Projects)
        {
            yield return new NewMatch(
                new NewMatchDto(
                    _url,
                    project.Name,
                    project.Id,
                    NewMatchType.Query,
                    "Query",
                    null,
                    null
                ),
                _iconCacheManager,
                _cache
            );

            foreach (var issueType in project.IssueTypes)
            {
                yield return new NewMatch(
                    new NewMatchDto(
                        _url,
                        project.Name,
                        project.Id,
                        NewMatchType.Issue,
                        issueType.Name,
                        issueType.Id,
                        issueType.IconUrl
                    ),
                    _iconCacheManager,
                    _cache
                );
            }
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
