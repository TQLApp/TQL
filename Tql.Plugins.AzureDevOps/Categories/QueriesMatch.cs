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

        var result = new List<IMatch>();

        foreach (var project in connection.Projects)
        {
            if (project.Features.Contains(AzureFeature.Boards))
            {
                var queries = await client.SearchQueriesAsync(
                    project.Id,
                    text,
                    cancellationToken: cancellationToken
                );

                result.AddRange(
                    queries
                        .Value
                        .Select(
                            p =>
                                factory.Create(
                                    new QueryMatchDto(dto.Url, project.Name, p.Id, p.Path, p.Name)
                                )
                        )
                );
            }
        }

        return result;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
