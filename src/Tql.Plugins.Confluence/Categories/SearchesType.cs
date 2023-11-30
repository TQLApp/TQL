using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Plugins.Confluence.Support;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

[RootMatchType]
internal class SearchesType(
    IMatchFactory<SearchesMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<SearchesMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Searches.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
