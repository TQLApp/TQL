using Launcher.Abstractions;
using Launcher.Plugins.AzureDevOps.Data;
using Launcher.Plugins.AzureDevOps.Services;
using Launcher.Plugins.AzureDevOps.Support;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi;

namespace Launcher.Plugins.AzureDevOps.Categories;

internal class QueriesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly Images _images;
    private readonly string _url;
    private readonly ICache<AzureData> _cache;
    private readonly IAzureDevOpsApi _api;

    public string Text { get; }
    public IImage Icon => _images.Boards;
    public MatchTypeId TypeId => TypeIds.Queries;

    public QueriesMatch(
        string text,
        Images images,
        string url,
        ICache<AzureData> cache,
        IAzureDevOpsApi api
    )
    {
        _images = images;
        _url = url;
        _cache = cache;
        _api = api;

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
                                _images
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
