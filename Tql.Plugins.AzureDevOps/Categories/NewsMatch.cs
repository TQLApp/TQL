using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Data;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Plugins.AzureDevOps.Support;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class NewsMatch : CachedMatch<AzureData>, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<NewMatch, NewMatchDto> _factory;

    public override string Text =>
        MatchUtils.GetMatchLabel(
            Labels.NewsType_Label,
            _configurationManager.Configuration,
            _dto.Url
        );

    public override ImageSource Icon => Images.Boards;
    public override MatchTypeId TypeId => TypeIds.News;
    public override string SearchHint => Labels.NewsMatch_SearchHint;

    public NewsMatch(
        RootItemDto dto,
        ICache<AzureData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<NewMatch, NewMatchDto> factory
    )
        : base(cache)
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    protected override IEnumerable<IMatch> Create(AzureData data)
    {
        foreach (var project in data.GetConnection(_dto.Url).Projects)
        {
            yield return _factory.Create(
                new NewMatchDto(_dto.Url, project.Name, NewMatchType.Query, null)
            );

            foreach (var workItemType in project.WorkItemTypes)
            {
                yield return _factory.Create(
                    new NewMatchDto(
                        _dto.Url,
                        project.Name,
                        NewMatchType.WorkItem,
                        workItemType.Name
                    )
                );
            }
        }
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
