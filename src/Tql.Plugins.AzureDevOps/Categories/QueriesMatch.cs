using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class QueriesMatch(
    RootItemDto dto,
    ConfigurationManager configurationManager,
    ICache<AzureData> cache,
    AzureDevOpsApi api,
    IMatchFactory<QueryMatch, QueryMatchDto> factory
) : ISearchableMatch, ISerializableMatch
{
    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.QueriesMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Queries;
    public string SearchHint => Labels.QueriesMatch_SearchHint;

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = await api.GetClient<WorkItemTrackingHttpClient>(dto.Url);
        var connection = (await cache.Get()).GetConnection(dto.Url);

        var results = (
            from project in connection.Projects
            where project.Features.Contains(AzureFeature.Boards)
            select (
                Project: project,
                Items: client.SearchQueriesAsync(
                    project.Id,
                    text,
                    cancellationToken: cancellationToken
                )
            )
        ).ToList();

        await Task.WhenAll(results.Select(p => p.Items));

        return from result in results
            from query in result.Items.Result.Value
            select factory.Create(
                new QueryMatchDto(dto.Url, result.Project.Name, query.Id, query.Path, query.Name)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
