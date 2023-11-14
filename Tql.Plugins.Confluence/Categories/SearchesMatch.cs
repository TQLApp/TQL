using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SearchesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<SearchMatch, SearchMatchDto> _factory;

    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.SearchesType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public ImageSource Icon => Images.Confluence;
    public MatchTypeId TypeId => TypeIds.Searches;
    public string SearchHint => Labels.SearchesMatch_SearchHint;

    public SearchesMatch(
        RootItemDto dto,
        ConfigurationManager configurationManager,
        IMatchFactory<SearchMatch, SearchMatchDto> factory
    )
    {
        _dto = dto;
        _configurationManager = configurationManager;
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

        var client = _configurationManager.GetClient(_dto.Url);

        string cql =
            $"siteSearch ~ \"{text.Replace("\"", "\\\"")}\" AND type in (\"space\",\"user\",\"attachment\",\"page\",\"blogpost\")";

        return SearchUtils.CreateMatches(
            _dto.Url,
            await client.SiteSearch(cql, 25, cancellationToken),
            _factory
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
