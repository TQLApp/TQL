using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi;
using Microsoft.VisualStudio.Services.Search.WebApi.Contracts.WorkItem;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class WorkItemsMatch : ISearchableMatch, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly AzureDevOpsApi _api;
    private readonly IMatchFactory<WorkItemMatch, WorkItemMatchDto> _factory;

    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.WorkItemsType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.WorkItems;

    public WorkItemsMatch(
        RootItemDto dto,
        ConfigurationManager configurationManager,
        AzureDevOpsApi api,
        IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory
    )
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _api = api;
        _factory = factory;
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
            var client = await _api.GetClient<WorkItemTrackingHttpClient>(_dto.Url);

            var workItem = await client.GetWorkItemAsync(
                workItemId,
                cancellationToken: cancellationToken
            );

            if (workItem == null)
                return Array.Empty<IMatch>();

            return new IMatch[]
            {
                _factory.Create(
                    new WorkItemMatchDto(
                        _dto.Url,
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

        var searchClient = await _api.GetClient<SearchHttpClient>(_dto.Url);

        var results = await searchClient.FetchWorkItemSearchResultsAsync(
            new WorkItemSearchRequest { SearchText = search, Top = 25 },
            cancellationToken: cancellationToken
        );

        return results.Results.Select(
            p =>
                _factory.Create(
                    new WorkItemMatchDto(
                        _dto.Url,
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
        return JsonSerializer.Serialize(_dto);
    }
}
