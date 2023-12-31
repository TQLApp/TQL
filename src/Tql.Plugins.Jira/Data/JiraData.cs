﻿namespace Tql.Plugins.Jira.Data;

internal record JiraData(ImmutableArray<JiraConnection> Connections)
{
    public JiraConnection GetConnection(string url) => Connections.Single(p => p.Url == url);
}

internal record JiraConnection(
    string Url,
    ImmutableArray<JiraDashboard> Dashboards,
    ImmutableArray<JiraProject> Projects,
    ImmutableArray<JiraBoard> Boards,
    ImmutableArray<JiraFilter> Filters
);

internal record JiraDashboard(string Id, string Name, string View);

internal record JiraProject(
    string Id,
    string Key,
    string Name,
    string AvatarUrl,
    string ProjectTypeKey,
    bool IsSimplified,
    string? Style,
    ImmutableArray<JiraIssueType> IssueTypes
);

internal record JiraIssueType(
    string Id,
    string Description,
    string IconUrl,
    string Name,
    bool IsSubTask
);

internal record JiraBoard(
    int Id,
    string Name,
    string Type,
    string LocationName,
    string ProjectKey,
    string ProjectTypeKey,
    string AvatarUrl,
    string FilterId,
    bool IsIssueListBacklog,
    bool IsSprintSupportEnabled,
    ImmutableArray<JiraQuickFilter> QuickFilters
);

internal record JiraQuickFilter(int Id, string Name, string Query);

internal record JiraFilter(string Id, string Name, string Jql, string ViewUrl);
