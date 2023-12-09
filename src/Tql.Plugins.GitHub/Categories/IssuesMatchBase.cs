using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesMatchBase<T>(
    RepositoryItemMatchDto dto,
    GitHubApi api,
    ICache<GitHubData> cache,
    IssueTypeQualifier type,
    IMatchFactory<T, IssueMatchDto> factory
) : ISearchableMatch, ISerializableMatch
    where T : IssueMatchBase
{
    public string Text =>
        MatchText.Path(
            dto.RepositoryName,
            type == IssueTypeQualifier.Issue
                ? Labels.IssuesMatchBase_IssueLabel
                : Labels.IssuesMatchBase_PullRequestLabel
        );

    public ImageSource Icon => type == IssueTypeQualifier.Issue ? Images.Issue : Images.PullRequest;

    public abstract MatchTypeId TypeId { get; }
    public abstract string SearchHint { get; }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        await context.DebounceDelay(cancellationToken);

        var client = await api.GetClient(dto.ConnectionId);

        var request = string.IsNullOrEmpty(text)
            ? new SearchIssuesRequest()
            : new SearchIssuesRequest(text);

        request.Type = type;
        request.Repos.Add(dto.Owner, dto.RepositoryName);

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
                            dto.ConnectionId,
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
