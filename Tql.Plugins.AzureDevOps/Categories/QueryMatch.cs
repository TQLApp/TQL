using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class QueryMatch : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    private readonly QueryMatchDto _dto;
    private readonly AzureDevOpsApi _api;
    private readonly IMatchFactory<WorkItemMatch, WorkItemMatchDto> _factory;

    public string Text => MatchText.Path(_dto.ProjectName, _dto.Path.Trim('\\').Replace('\\', '/'));

    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Query;

    public QueryMatch(
        QueryMatchDto dto,
        AzureDevOpsApi api,
        IMatchFactory<WorkItemMatch, WorkItemMatchDto> factory
    )
    {
        _dto = dto;
        _api = api;
        _factory = factory;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.GetUrl());

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
            return await context.FilterAsync(matches);

        return matches;
    }

    private async Task<ImmutableArray<IMatch>> ExecuteQuery(IServiceProvider serviceProvider)
    {
        var client = await _api.GetClient<WorkItemTrackingHttpClient>(_dto.Url);

        // GetWorkItemsAsync allows for a maximum of 200 work items.
        // Limit to that.
        var result = await client.QueryByIdAsync(_dto.Id, top: 200);

        return await QueryUtils.GetWorkItemsByIds(client, _dto.Url, result, _factory);
    }
}

internal record QueryMatchDto(string Url, string ProjectName, Guid Id, string Path, string Name)
{
    public string GetUrl() =>
        $"{Url.TrimEnd('/')}/{Uri.EscapeDataString(ProjectName)}/_queries/query-edit/{Uri.EscapeDataString(Id.ToString())}";
};
