using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi.Contracts.WorkItem;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class WorkItemsMatch : ISearchableMatch, ISerializableMatch
{
    private readonly string _url;
    private readonly ICache<AzureData> _cache;
    private readonly AzureDevOpsApi _api;
    private readonly AzureWorkItemIconManager _iconManager;

    public string Text { get; }
    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.WorkItems;

    public WorkItemsMatch(
        string text,
        string url,
        ICache<AzureData> cache,
        AzureDevOpsApi api,
        AzureWorkItemIconManager iconManager
    )
    {
        _url = url;
        _cache = cache;
        _api = api;
        _iconManager = iconManager;

        Text = text;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        var isId = int.TryParse(text, out int workItemId);

        if (isId)
            context.SuppressPreliminaryResults();

        await context.DebounceDelay(cancellationToken);

        if (isId)
        {
            var client = await _api.GetClient<WorkItemTrackingHttpClient>(_url);

            var workItem = await client.GetWorkItemAsync(
                workItemId,
                cancellationToken: cancellationToken
            );

            if (workItem == null)
                return Array.Empty<IMatch>();

            return new IMatch[]
            {
                new WorkItemMatch(
                    new WorkItemMatchDto(
                        _url,
                        (string)workItem.Fields["System.TeamProject"],
                        workItem.Id!.Value,
                        (string)workItem.Fields["System.WorkItemType"],
                        (string)workItem.Fields["System.Title"]
                    ),
                    _iconManager
                )
            };
        }

        // The search feature requires at least three characters.

        var search = text.Trim();
        if (search.Length < 3)
            return Array.Empty<IMatch>();

        var searchClient = await _api.GetClient<SearchHttpClient>(_url);

        var results = await searchClient.FetchWorkItemSearchResultsAsync(
            new WorkItemSearchRequest { SearchText = search, Top = 25 },
            cancellationToken: cancellationToken
        );

        return results.Results.Select(
            p =>
                new WorkItemMatch(
                    new WorkItemMatchDto(
                        _url,
                        p.Project.Name,
                        int.Parse(p.Fields["system.id"]),
                        p.Fields["system.workitemtype"],
                        p.Fields["system.title"]
                    ),
                    _iconManager
                )
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
