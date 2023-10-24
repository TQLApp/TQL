using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class DashboardsMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly string _url;

    public override string Text { get; }
    public override ImageSource Icon => Images.Dashboard;
    public override MatchTypeId TypeId => TypeIds.Dashboards;

    public DashboardsMatch(string text, string url, ICache<JiraData> cache)
        : base(cache)
    {
        _url = url;

        Text = text;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        return from dashboard in data.GetConnection(_url).Dashboards
            select new DashboardMatch(
                new DashboardMatchDto(_url, dashboard.Id, dashboard.Name, dashboard.View)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
