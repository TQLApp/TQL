using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SearchType(
    IMatchFactory<SearchMatch, SearchMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<SearchMatch, SearchMatchDto>(factory)
{
    public override Guid Id => TypeIds.Search.Id;

    protected override bool IsValid(SearchMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
