using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class FiltersMatch : CachedMatch<JiraData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<FilterMatch, FilterMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.FiltersMatch_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Filters;
    public override MatchTypeId TypeId => TypeIds.Filters;
    public override string SearchHint => Labels.FiltersMatch_SearchHint;

    public FiltersMatch(
        RootItemDto dto,
        ICache<JiraData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<FilterMatch, FilterMatchDto> factory
    )
        : base(cache)
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        return from filter in data.GetConnection(_dto.Url).Filters
            select _factory.Create(
                new FilterMatchDto(_dto.Url, filter.Name, filter.ViewUrl, filter.Jql)
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
