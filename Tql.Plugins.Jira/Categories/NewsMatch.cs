using Tql.Abstractions;
using Tql.Plugins.Jira.Data;
using Tql.Plugins.Jira.Services;
using Tql.Plugins.Jira.Support;
using Tql.Utilities;

namespace Tql.Plugins.Jira.Categories;

internal class NewsMatch : CachedMatch<JiraData>, ISerializableMatch
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

    public override ImageSource Icon => Images.Issues;
    public override MatchTypeId TypeId => TypeIds.News;

    public NewsMatch(
        RootItemDto dto,
        ICache<JiraData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<NewMatch, NewMatchDto> factory
    )
        : base(cache)
    {
        _dto = dto;
        _configurationManager = configurationManager;
        _factory = factory;
    }

    protected override IEnumerable<IMatch> Create(JiraData data)
    {
        foreach (var project in data.GetConnection(_dto.Url).Projects)
        {
            yield return _factory.Create(
                new NewMatchDto(
                    _dto.Url,
                    project.Name,
                    project.Id,
                    NewMatchType.Query,
                    Labels.NewsMatch_Query,
                    null,
                    null
                )
            );

            foreach (var issueType in project.IssueTypes)
            {
                yield return _factory.Create(
                    new NewMatchDto(
                        _dto.Url,
                        project.Name,
                        project.Id,
                        NewMatchType.Issue,
                        issueType.Name,
                        issueType.Id,
                        issueType.IconUrl
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
