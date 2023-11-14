using Tql.Abstractions;
using Tql.Plugins.MicrosoftTeams.Services;
using Tql.Utilities;

namespace Tql.Plugins.MicrosoftTeams.Categories;

internal abstract class PeopleTypeBase<TCategory, TMatch> : MatchType<TCategory, RootItemDto>
    where TCategory : PeopleMatchBase<TMatch>
    where TMatch : IMatch
{
    private readonly ConfigurationManager _configurationManager;

    protected PeopleTypeBase(
        ConfigurationManager configurationManager,
        IMatchFactory<TCategory, RootItemDto> factory
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RootItemDto dto) => _configurationManager.HasDirectory(dto.Id);
}
