using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Plugins.AzureDevOps.Support;
using Launcher.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class QueryMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    private readonly QueryMatchDto _dto;
    private readonly AzureDevOpsApi _api;
    private readonly AzureWorkItemIconManager _iconManager;

    public string Text =>
        System.IO.Path.Combine(_dto.ProjectName, _dto.Path.Trim('\\')).Replace('\\', '/');
    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Query;

    public QueryMatch(QueryMatchDto dto, AzureDevOpsApi api, AzureWorkItemIconManager iconManager)
    {
        _dto = dto;
        _api = api;
        _iconManager = iconManager;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().LaunchUrl(_dto.GetUrl());

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.GetUrl());

        return Task.CompletedTask;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        var cache = context.GetDataCached($"{GetType().FullName}:{_dto}", ExecuteQuery);

        if (!cache.IsCompleted)
            await context.DebounceDelay(cancellationToken);

        var matches = await cache;

        if (!text.IsWhiteSpace())
            return context.Filter(matches);

        return matches;
    }

    private async Task<ImmutableArray<IMatch>> ExecuteQuery(IServiceProvider serviceProvider)
    {
        var client = await _api.GetClient<WorkItemTrackingHttpClient>(_dto.Url);

        // GetWorkItemsAsync allows for a maximum of 200 work items.
        // Limit to that.
        var result = await client.QueryByIdAsync(_dto.Id, top: 200);

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
            .Select(
                p =>
                    new WorkItemMatch(
                        new WorkItemMatchDto(
                            _dto.Url,
                            (string)p.Fields["System.TeamProject"],
                            p.Id!.Value,
                            (string)p.Fields["System.WorkItemType"],
                            (string)p.Fields["System.Title"]
                        ),
                        _iconManager
                    )
            )
            .ToImmutableArray<IMatch>();
    }
}

internal record QueryMatchDto(string Url, string ProjectName, Guid Id, string Path, string Name)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_queries/query-edit/{Uri.EscapeDataString(Id.ToString())}";
};
