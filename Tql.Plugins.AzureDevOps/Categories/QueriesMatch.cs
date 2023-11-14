using Microsoft.TeamFoundation.WorkItemTracking.WebApi;
using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class QueriesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly ICache<AzureData> _cache;
    private readonly AzureDevOpsApi _api;
    private readonly IMatchFactory<QueryMatch, QueryMatchDto> _factory;

    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.QueriesType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public ImageSource Icon => Images.Boards;
    public MatchTypeId TypeId => TypeIds.Queries;

    public QueriesMatch(
        RootItemDto dto,
        ConfigurationManager configurationManager,
        ICache<AzureData> cache,
        AzureDevOpsApi api,
        IMatchFactory<QueryMatch, QueryMatchDto> factory
    )
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _cache = cache;
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

        await context.DebounceDelay(cancellationToken);

        var client = await _api.GetClient<WorkItemTrackingHttpClient>(_dto.Url);
        var connection = (await _cache.Get()).GetConnection(_dto.Url);

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
                            _factory.Create(
                                new QueryMatchDto(_dto.Url, project.Name, p.Id, p.Path, p.Name)
                            )
                    )
                );
            }
        }

        return result;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
