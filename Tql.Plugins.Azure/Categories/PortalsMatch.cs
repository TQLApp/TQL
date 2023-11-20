using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Azure.ResourceManager.Resources;
using Tql.Abstractions;
using Tql.Plugins.Azure.Services;
using Tql.Plugins.Azure.Support;
using Tql.Utilities;

namespace Tql.Plugins.Azure.Categories;

internal class PortalsMatch : ISearchableMatch, ISerializableMatch
{
    private const int SearchResultCount = 30;

    private static readonly string[] QueryTemplates =
    {
        LoadQuery("ResourceGroupQuery.txt"),
        LoadQuery("ResourceQuery.txt")
    };

    private static string LoadQuery(string resourceName)
    {
        using var stream = typeof(PortalsType)
            .Assembly
            .GetManifestResourceStream($"{typeof(PortalsType).Namespace}.{resourceName}");
        using var reader = new StreamReader(stream!);

        return reader.ReadToEnd();
    }

    private readonly AzureApi _api;
    private readonly IMatchFactory<PortalMatch, PortalMatchDto> _factory;
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;

    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.PortalsMatch_Label,
            _configurationManager.Configuration,
            _dto.Id
        );

    public ImageSource Icon => Images.Azure;
    public MatchTypeId TypeId => TypeIds.Portals;
    public string SearchHint => Labels.PortalsMatch_SearchHint;

    public PortalsMatch(
        RootItemDto dto,
        ConfigurationManager configurationManager,
        AzureApi api,
        IMatchFactory<PortalMatch, PortalMatchDto> factory
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

        await context.DebounceDelay(cancellationToken);

        // We run two queries. One for the main resources and one for the
        // resource groups.

        var client = await _api.GetClient(_dto.Id);

        // All tenants seem to return the same results.
        var tenant = client.GetTenants().First();

        var tasks = QueryTemplates
            .Select(p => RunQuery(text, tenant, p, cancellationToken))
            .ToList();

        await Task.WhenAll(tasks);

        return context.Filter(tasks.SelectMany(p => p.Result));
    }

    private async Task<List<IMatch>> RunQuery(
        string text,
        TenantResource tenant,
        string queryTemplate,
        CancellationToken cancellationToken
    )
    {
        var query = string.Format(
            queryTemplate,
            text.Replace("\\", "\\\\").Replace("'", "\\'"),
            SearchResultCount
        );

        var response = await tenant.GetResourcesAsync(
            new ResourceQueryContent(query),
            cancellationToken
        );

        var resources = response
            .Value
            .Data
            .ToObjectFromJson<List<ResourceDto>>(
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

        return resources
            .Select(
                p =>
                    _factory.Create(
                        new PortalMatchDto(
                            _dto.Id,
                            tenant.Data.DefaultDomain,
                            p.Id,
                            p.Name,
                            p.Type,
                            p.Kind,
                            p.SubscriptionId,
                            p.ResourceGroup,
                            p.NormalizedName
                        )
                    )
            )
            .ToList<IMatch>();
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    private record ResourceDto(
        string Id,
        string Name,
        string Type,
        string Kind,
        Guid SubscriptionId,
        string ResourceGroup,
        string NormalizedName
    );
}
