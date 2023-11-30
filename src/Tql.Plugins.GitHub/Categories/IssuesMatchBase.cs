using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesMatchBase<T>(
    RootItemDto dto,
    GitHubApi api,
    ICache<GitHubData> cache,
    IssueTypeQualifier type,
    ConfigurationManager configurationManager,
    IMatchFactory<T, IssueMatchDto> factory
) : ISearchableMatch, ISerializableMatch
    where T : IssueMatchBase
{
    public string Text =>
        MatchUtils.GetMatchLabel(
            type == IssueTypeQualifier.Issue
                ? Labels.IssuesMatchBase_IssueLabel
                : Labels.IssuesMatchBase_PullRequestLabel,
            type == IssueTypeQualifier.Issue
                ? Labels.IssuesMatchBase_MyIssueLabel
                : Labels.IssuesMatchBase_MyPullRequestLabel,
            configurationManager.Configuration,
            dto
        );

    public ImageSource Icon => Images.Issue;
    public abstract MatchTypeId TypeId { get; }
    public abstract string SearchHint { get; }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (dto.Scope == RootItemScope.Global && text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = await api.GetClient(dto.Id);

        var request = new SearchIssuesRequest(await GitHubUtils.GetSearchPrefix(dto, cache) + text)
        {
            Type = type
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
                    factory.Create(
                        new IssueMatchDto(
                            dto.Id,
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
        return JsonSerializer.Serialize(dto);
    }
}
