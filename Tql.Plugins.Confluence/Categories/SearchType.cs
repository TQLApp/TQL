using Tql.Abstractions;
using Tql.Plugins.Confluence.Services;
using Tql.Utilities;

namespace Tql.Plugins.Confluence.Categories;

internal class SearchType : MatchType<SearchMatch, SearchMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Search.Id;

    public SearchType(
        IMatchFactory<SearchMatch, SearchMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(SearchMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
