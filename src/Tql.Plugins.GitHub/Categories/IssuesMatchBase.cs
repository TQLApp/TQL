using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal abstract class IssuesMatchBase<T>(
    RepositoryItemMatchDto dto,
    GitHubApi api,
    IssueTypeQualifier type,
    IMatchFactory<T, IssueMatchDto> factory,
    IssueTypeBase<T> issueType
) : IRunnableMatch, ICopyableMatch, ISearchableMatch, ISerializableMatch
    where T : IssueMatchBase
{
    public string Text =>
        MatchText.Path(
            $"{dto.Owner}/{dto.RepositoryName}",
            type == IssueTypeQualifier.Issue
                ? Labels.IssuesMatchBase_IssueLabel
                : Labels.IssuesMatchBase_PullRequestLabel
        );

    public ImageSource Icon => type == IssueTypeQualifier.Issue ? Images.Issue : Images.PullRequest;

    public abstract MatchTypeId TypeId { get; }
    public abstract string SearchHint { get; }

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(GetUrl());

        return Task.CompletedTask;
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, GetUrl());

        return Task.CompletedTask;
    }

    private string GetUrl() =>
        $"{dto.GetUrl()}/{(type == IssueTypeQualifier.Issue ? "issues" : "pulls")}";

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

        var dtos = response
            .Items.Select(
                p =>
                    new IssueMatchDto(
                        dto.ConnectionId,
                        GitHubUtils.GetRepositoryName(p.HtmlUrl),
                        p.Number,
                        p.Title,
                        p.HtmlUrl,
                        IssueMatchStateUtils.FromIssue(p)
                    )
            )
            .ToList();

        issueType.UpdateCache(dtos);

        return dtos.Select(factory.Create);
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
