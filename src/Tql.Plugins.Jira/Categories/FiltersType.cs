using Tql.Abstractions;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

[RootMatchType]
internal class FiltersType(
    IMatchFactory<FiltersMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<FiltersMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Filters.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
