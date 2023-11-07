using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class FilterMatch
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch,
        ISearchableMatch,
        IHasSearchHint
{
    private readonly FilterMatchDto _dto;
    private readonly IconCacheManager _iconCacheManager;
    private readonly ConfigurationManager _configurationManager;

    public string Text => _dto.Name;
    public ImageSource Icon => Images.Filters;
    public MatchTypeId TypeId => TypeIds.Filter;
    public string SearchHint => Labels.FilterMatch_SearchHint;

    public FilterMatch(
        FilterMatchDto dto,
        IconCacheManager iconCacheManager,
        ConfigurationManager configurationManager
    )
    {
        _dto = dto;
        _iconCacheManager = iconCacheManager;
        _configurationManager = configurationManager;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.ViewUrl);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.ViewUrl);

        return Task.CompletedTask;
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

        var client = _configurationManager.GetClient(_dto.Url);

        var issues = await client.SearchIssues(_dto.Jql, 100, cancellationToken);

        return IssueUtils.CreateMatches(_dto.Url, issues, _iconCacheManager);
    }
}

internal record FilterMatchDto(string Url, string Name, string ViewUrl, string Jql);
