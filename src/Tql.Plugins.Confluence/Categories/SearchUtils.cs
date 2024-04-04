using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;

namespace Tql.Plugins.Confluence.Categories;

internal static class SearchUtils
{
    public static IEnumerable<IMatch> CreateMatches(
        string url,
        IEnumerable<ConfluenceSiteSearchDto> searchResults,
        IMatchFactory<SearchMatch, SearchMatchDto> factory
    )
    {
        return searchResults.Select(p =>
            factory.Create(
                new SearchMatchDto(
                    url,
                    p.ResultGlobalContainer?.Title,
                    p.Title,
                    p.Content?.Links.TinyUI ?? p.Url
                )
            )
        );
    }
}
