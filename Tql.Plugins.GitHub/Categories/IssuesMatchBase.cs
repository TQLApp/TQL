using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesMatchBase<T> : ISearchableMatch, ISerializableMatch
    where T : IssueMatchBase
{
    private readonly RootItemDto _dto;
    private readonly GitHubApi _api;
    private readonly IssueTypeQualifier _type;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<T, IssueMatchDto> _factory;
    private readonly ICache<GitHubData> _cache;

    public string Text =>
        MatchUtils.GetMatchLabel(
            _type == IssueTypeQualifier.Issue
                ? Labels.IssuesMatchBase_IssueLabel
                : Labels.IssuesMatchBase_PullRequestLabel,
            _type == IssueTypeQualifier.Issue
                ? Labels.IssuesMatchBase_MyIssueLabel
                : Labels.IssuesMatchBase_MyPullRequestLabel,
            _configurationManager.Configuration,
            _dto
        );

    public ImageSource Icon => Images.Issue;
    public abstract MatchTypeId TypeId { get; }
    public abstract string SearchHint { get; }

    protected IssuesMatchBase(
        RootItemDto dto,
        GitHubApi api,
        ICache<GitHubData> cache,
        IssueTypeQualifier type,
        ConfigurationManager configurationManager,
        IMatchFactory<T, IssueMatchDto> factory
    )
    {
        _dto = dto;
        _api = api;
        _type = type;
        _configurationManager = configurationManager;
        _factory = factory;
        _cache = cache;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (_dto.Scope == RootItemScope.Global && text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = await _api.GetClient(_dto.Id);

        var request = new SearchIssuesRequest(
            await GitHubUtils.GetSearchPrefix(_dto, _cache) + text
        )
        {
            Type = _type
        };

        if (text.IsWhiteSpace())
        {
            request.SortField = IssueSearchSort.Created;
            request.Order = SortDirection.Descending;
        }

        var response = await client.Search.SearchIssues(request);

        cancellationToken.ThrowIfCancellationRequested();

        return response
            .Items
            .Select(
                p =>
                    _factory.Create(
                        new IssueMatchDto(
                            _dto.Id,
                            GitHubUtils.GetRepositoryName(p.HtmlUrl),
                            p.Number,
                            p.Title,
                            p.HtmlUrl,
                            p.State.Value
                        )
                    )
            );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
