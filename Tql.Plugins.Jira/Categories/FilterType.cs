using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class FilterType(
    IMatchFactory<FilterMatch, FilterMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<FilterMatch, FilterMatchDto>(factory)
{
    public override Guid Id => TypeIds.Filter.Id;

    protected override bool IsValid(FilterMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
