using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesMatchBase : ISearchableMatch, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly GitHubApi _api;
    private readonly IssueTypeQualifier _type;
    private readonly ICache<GitHubData> _cache;

    public string Text { get; }
    public ImageSource Icon => Images.Issue;
    public abstract MatchTypeId TypeId { get; }

    protected IssuesMatchBase(
        string text,
        RootItemDto dto,
        GitHubApi api,
        ICache<GitHubData> cache,
        IssueTypeQualifier type
    )
    {
        _dto = dto;
        _api = api;
        _type = type;
        _cache = cache;

        Text = text;
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

        return response.Items.Select(
            p =>
                CreateIssue(
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

    protected abstract IssueMatchBase CreateIssue(IssueMatchDto dto);
}
