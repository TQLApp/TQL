using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class FiltersMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly string _url;
    private readonly IconCacheManager _iconCacheManager;
    private readonly ConfigurationManager _configurationManager;

    public override string Text { get; }
    public override ImageSource Icon => Images.Filters;
    public override MatchTypeId TypeId => TypeIds.Filters;

    public FiltersMatch(
        string text,
        string url,
        ICache<JiraData> cache,
        IconCacheManager iconCacheManager,
        ConfigurationManager configurationManager
    )
        : base(cache)
    {
        _url = url;
        _iconCacheManager = iconCacheManager;
        _configurationManager = configurationManager;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        return from filter in data.GetConnection(_url).Filters
            select new FilterMatch(
                new FilterMatchDto(_url, filter.Name, filter.ViewUrl, filter.Jql),
                _iconCacheManager,
                _configurationManager
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
