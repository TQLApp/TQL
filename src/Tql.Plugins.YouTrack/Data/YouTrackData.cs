namespace Tql.Plugins.YouTrack.Data;

internal record YouTrackData(ImmutableArray<YouTrackConnection> Connections)
{
    public YouTrackConnection GetConnection(string url) => Connections.Single(p => p.Url == url);
}

/// <param name="UnavailableEndpoints">
/// The endpoints that answered 403 or 404. Their lists are empty, and the
/// plugin leaves their categories out for this connection.
/// </param>
internal record YouTrackConnection(
    string Url,
    ImmutableArray<YouTrackProject> Projects,
    ImmutableArray<YouTrackAgileBoard> AgileBoards,
    ImmutableArray<YouTrackSavedSearch> SavedSearches,
    ImmutableArray<YouTrackDashboard> Dashboards,
    ImmutableArray<YouTrackEndpoint> UnavailableEndpoints
);

internal record YouTrackProject(string Id, string ShortName, string Name, string? IconUrl);

internal record YouTrackAgileBoard(string Id, string Name);

internal record YouTrackSavedSearch(string Id, string Name, string Query);

internal record YouTrackDashboard(string Id, string Name);

/// <summary>
/// The endpoints the cache loads, each of which a connection may lack.
/// </summary>
internal enum YouTrackEndpoint
{
    Projects,
    AgileBoards,
    SavedSearches,
    Dashboards
}
