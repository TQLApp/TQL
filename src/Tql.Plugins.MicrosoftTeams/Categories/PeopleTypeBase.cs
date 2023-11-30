using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal abstract class PeopleTypeBase<TCategory, TMatch>(
    ConfigurationManager configurationManager,
    IMatchFactory<TCategory, RootItemDto> factory
) : MatchType<TCategory, RootItemDto>(factory)
    where TCategory : PeopleMatchBase<TMatch>
    where TMatch : IMatch
{
    protected override bool IsValid(RootItemDto dto) => configurationManager.HasDirectory(dto.Id);
}
