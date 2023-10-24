namespace Tql.Plugins.Jira.Data;

internal record JiraData(ImmutableArray<JiraConnection> Connections)
{
    public JiraConnection GetConnection(string url) => Connections.Single(p => p.Url == url);
}

internal record JiraConnection(string Url, ImmutableArray<JiraDashboard> Dashboards);

internal record JiraDashboard(string Id, string Name, string View);
