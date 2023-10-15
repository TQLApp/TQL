using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesMatchBase : ISearchableMatch, ISerializableMatch
{
    private readonly Guid _id;
    private readonly GitHubApi _api;
    private readonly IssueTypeQualifier _type;

    public string Text { get; }
    public ImageSource Icon => Images.Issue;
    public abstract MatchTypeId TypeId { get; }

    protected IssuesMatchBase(string text, Guid id, GitHubApi api, IssueTypeQualifier type)
    {
        _id = id;
        _api = api;
        _type = type;

        Text = text;
    }

    public async Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = await _api.GetClient(_id);

        var response = await client.Search.SearchIssues(
            new SearchIssuesRequest(text) { Type = _type }
        );

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(
            p =>
                CreateIssue(
                    new IssueMatchDto(
                        _id,
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
        return JsonSerializer.Serialize(new RootItemDto(_id));
    }

    protected abstract IssueMatchBase CreateIssue(IssueMatchDto dto);
}
