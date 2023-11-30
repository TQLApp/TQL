using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SearchesMatch(
    RootItemDto dto,
    ConfigurationManager configurationManager,
    IMatchFactory<SearchMatch, SearchMatchDto> factory
) : ISearchableMatch, ISerializableMatch
{
    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.SearchesMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public ImageSource Icon => Images.Confluence;
    public MatchTypeId TypeId => TypeIds.Searches;
    public string SearchHint => Labels.SearchesMatch_SearchHint;

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = configurationManager.GetClient(dto.Url);

        string cql =
            $"siteSearch ~ \"{text.Replace("\"", "\\\"")}\" AND type in (\"space\",\"user\",\"attachment\",\"page\",\"blogpost\")";

        return SearchUtils.CreateMatches(
            dto.Url,
            await client.SiteSearch(cql, 25, cancellationToken),
            factory
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
