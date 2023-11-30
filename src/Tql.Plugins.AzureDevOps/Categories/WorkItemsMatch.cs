using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi.Contracts.WorkItem;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class WorkItemsMatch(
    RootItemDto dto,
    ConfigurationManager configurationManager,
    AzureDevOpsApi api,
    IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory
) : ISearchableMatch, ISerializableMatch
{
    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.WorkItemsMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.WorkItems;
    public string SearchHint => Labels.WorkItemsMatch_SearchHint;

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
            var client = await api.GetClient<WorkItemTrackingHttpClient>(dto.Url);

            var workItem = await client.GetWorkItemAsync(
                workItemId,
                cancellationToken: cancellationToken
            );

            if (workItem == null)
                return Array.Empty<IMatch>();

            return new IMatch[]
            {
                factory.Create(
                    new WorkItemMatchDto(
                        dto.Url,
                        (string)workItem.Fields["System.TeamProject"],
                        workItem.Id!.Value,
                        (string)workItem.Fields["System.WorkItemType"],
                        (string)workItem.Fields["System.Title"]
                    )
                )
            };
        }

        // The search feature requires at least three characters.

        var search = text.Trim();
        if (search.Length < 3)
            return Array.Empty<IMatch>();

        var searchClient = await api.GetClient<SearchHttpClient>(dto.Url);

        var results = await searchClient.FetchWorkItemSearchResultsAsync(
            new WorkItemSearchRequest { SearchText = search, Top = 25 },
            cancellationToken: cancellationToken
        );

        return results
            .Results
            .Select(
                p =>
                    factory.Create(
                        new WorkItemMatchDto(
                            dto.Url,
                            p.Project.Name,
                            int.Parse(p.Fields["system.id"]),
                            p.Fields["system.workitemtype"],
                            p.Fields["system.title"]
                        )
                    )
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
