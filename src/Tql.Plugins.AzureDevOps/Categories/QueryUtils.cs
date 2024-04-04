using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal static class QueryUtils
{
    public static async Task<ImmutableArray<IMatch>> SearchInBacklog(
        string connectionUrl,
        string projectName,
        string teamName,
        string backlogName,
        string text,
        AzureDevOpsApi api,
        AzureData cache,
        IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory,
        CancellationToken cancellationToken
    )
    {
        var connection = cache.GetConnection(connectionUrl);
        var project = connection.Projects.Single(p => p.Name == projectName);
        var team = project.Teams.Single(p => p.Name == teamName);
        var backlog = project.Backlogs.Single(p => p.Name == backlogName);

        var sb = new StringBuilder();

        sb.AppendLine(
            $"""
            SELECT [System.Id]
                FROM WorkItems
                WHERE [System.TeamProject] = {Escape(project.Name)}
                    AND [System.WorkItemType] IN ({string.Join(
                ", ",
                backlog.WorkItemTypes.Select(Escape)
            )})
            """
        );

        if (!text.IsEmpty())
        {
            sb.AppendLine(
                $"""
                AND (
                        [System.Title] CONTAINS WORDS {Escape(text)}
                        OR [System.Description] CONTAINS WORDS {Escape(text)}
                    )
                """
            );
        }

        sb.AppendLine(
            """
                AND (
            """
        );

        for (var i = 0; i < team.AreaPaths.Length; i++)
        {
            if (i > 0)
                sb.Append("        OR ");

            sb.AppendLine(
                $"[System.AreaPath] {(team.AreaPaths[i].IncludeChildren ? "UNDER" : "=")} {Escape(team.AreaPaths[i].Name)}"
            );
        }

        sb.AppendLine(
            """
                )
            """
        );

        var client = await api.GetClient<WorkItemTrackingHttpClient>(connectionUrl);

        var result = await client.QueryByWiqlAsync(
            new Wiql { Query = sb.ToString() },
            cancellationToken: cancellationToken
        );

        return await GetWorkItemsByIds(client, connectionUrl, result, factory);
    }

    public static async Task<ImmutableArray<IMatch>> GetWorkItemsByIds(
        WorkItemTrackingHttpClient client,
        string connectionUrl,
        WorkItemQueryResult result,
        IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory
    )
    {
        var ids = result.WorkItems.Select(p => p.Id).ToList();

        if (ids.Count == 0)
            return ImmutableArray<IMatch>.Empty;

        var workItems = await client.GetWorkItemsAsync(
            ids,
            new[] { "System.Title", "System.WorkItemType", "System.TeamProject" },
            result.AsOf,
            errorPolicy: WorkItemErrorPolicy.Omit
        );

        return workItems
            .Select(p =>
                factory.Create(
                    new WorkItemMatchDto(
                        connectionUrl,
                        (string)p.Fields["System.TeamProject"],
                        p.Id!.Value,
                        (string)p.Fields["System.WorkItemType"],
                        (string)p.Fields["System.Title"]
                    )
                )
            )
            .ToImmutableArray<IMatch>();
    }

    private static string Escape(string value) =>
        $"'{value.Replace("\\", "\\\\").Replace("'", "\\'")}'";
}
