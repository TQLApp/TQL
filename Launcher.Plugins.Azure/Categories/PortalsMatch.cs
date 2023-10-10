﻿using System.Text.Json.Serialization;
using Launcher.Abstractions;
using Launcher.Plugins.Azure.Services;
using System.Threading.Tasks.Sources;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.ResourceGraph;
using Azure.ResourceManager.ResourceGraph.Models;
using Launcher.Plugins.Azure.Support;

namespace Launcher.Plugins.Azure.Categories;

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
        using var stream = typeof(PortalsType).Assembly.GetManifestResourceStream(
            $"{typeof(PortalsType).Namespace}.{resourceName}"
        );
        using var reader = new StreamReader(stream!);

        return reader.ReadToEnd();
    }

    private readonly Images _images;
    private readonly Guid _id;
    private readonly AzureApi _api;
    private readonly IImageFactory _imageFactory;

    public string Text { get; }
    public IImage Icon => _images.Azure;
    public MatchTypeId TypeId => TypeIds.Portals;

    public PortalsMatch(
        string text,
        Images images,
        Guid id,
        AzureApi api,
        IImageFactory imageFactory
    )
    {
        _images = images;
        _id = id;
        _api = api;
        _imageFactory = imageFactory;

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

        // We run two queries. One for the main resources and one for the
        // resource groups.

        var client = await _api.GetClient(_id);

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

        var resources = response.Value.Data.ToObjectFromJson<List<ResourceDto>>(
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );

        return resources
            .Select(
                p =>
                    new PortalMatch(
                        new PortalMatchDto(
                            _id,
                            tenant.Data.DefaultDomain,
                            p.Id,
                            p.Name,
                            p.Type,
                            p.Kind,
                            p.SubscriptionId,
                            p.ResourceGroup,
                            p.NormalizedName
                        ),
                        _images,
                        _imageFactory
                    )
            )
            .ToList<IMatch>();
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(new RootItemDto(_id));
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