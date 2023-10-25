using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;

namespace Tql.Plugins.Confluence.Categories;

internal static class SearchUtils
{
    public static IEnumerable<IMatch> CreateMatches(
        string url,
        IEnumerable<ConfluenceSiteSearchDto> searchResults
    )
    {
        return searchResults.Select(
            p =>
                new SearchMatch(
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
