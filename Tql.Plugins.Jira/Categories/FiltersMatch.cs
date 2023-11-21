using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class FiltersMatch(
    RootItemDto dto,
    ICache<JiraData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<FilterMatch, FilterMatchDto> factory
) : CachedMatch<JiraData>(cache), ISerializableMatch
{
    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.FiltersMatch_Label,
            configurationManager.Configuration,
            dto.Url
        );

    public override ImageSource Icon => Images.Filters;
    public override MatchTypeId TypeId => TypeIds.Filters;
    public override string SearchHint => Labels.FiltersMatch_SearchHint;

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        return from filter in data.GetConnection(dto.Url).Filters
            select factory.Create(
                new FilterMatchDto(dto.Url, filter.Name, filter.ViewUrl, filter.Jql)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
