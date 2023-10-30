using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class QueriesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly string _url;
    private readonly ICache<AzureData> _cache;
    private readonly AzureDevOpsApi _api;
    private readonly AzureWorkItemIconManager _iconManager;

    public string Text { get; }
    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Queries;

    public QueriesMatch(
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

        await context.DebounceDelay(cancellationToken);

        var client = await _api.GetClient<WorkItemTrackingHttpClient>(_url);
        var connection = (await _cache.Get()).GetConnection(_url);

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
                    queries.Value.Select(
                        p =>
                            new QueryMatch(
                                new QueryMatchDto(_url, project.Name, p.Id, p.Path, p.Name),
                                _api,
                                _iconManager
                            )
                    )
                );
            }
        }

        return result;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_url));
    }
}
