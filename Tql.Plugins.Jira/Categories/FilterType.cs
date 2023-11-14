using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class FilterType : MatchType<FilterMatch, FilterMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Filter.Id;

    public FilterType(
        IMatchFactory<FilterMatch, FilterMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(FilterMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
