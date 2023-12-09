using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class MilestoneMatch(
    MilestoneMatchDto dto,
    GitHubApi api,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => MatchText.Path($"{dto.Owner}/{dto.RepositoryName}", dto.Title);
    public ImageSource Icon => Images.Milestone;
    public MatchTypeId TypeId => TypeIds.Milestone;
    public string SearchHint => Labels.MilestoneMatch_SearchHint;

    public Task Run(IServiceProvider serviceProvider, IWin32Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(dto.Url);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        var clipboard = serviceProvider.GetRequiredService<IClipboard>();

        clipboard.CopyMarkdown(Text, dto.Url);

        return Task.CompletedTask;
    }

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

        request.Milestone = dto.Title;
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
}

internal record MilestoneMatchDto(
    Guid ConnectionId,
    string Owner,
    string RepositoryName,
    string Title,
    string Url
);
