using System.Windows.Forms;
using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class RepositoryMatch(
    RepositoryMatchDto dto,
    GitHubApi api,
    IMatchFactory<IssueMatch, IssueMatchDto> factory
) : IRunnableMatch, ISerializableMatch, ICopyableMatch, ISearchableMatch
{
    public string Text => dto.Name;
    public ImageSource Icon => Images.Repository;
    public MatchTypeId TypeId => TypeIds.Repository;

    public string SearchHint => Labels.RepositoryMatch_SearchHint;

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
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, dto.Url);

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

        var request = text.IsWhiteSpace()
            ? new SearchIssuesRequest()
            : new SearchIssuesRequest(text);

        request.Repos.Add(dto.Name);

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

internal record RepositoryMatchDto(Guid ConnectionId, string Name, string Url);
