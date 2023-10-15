using Microsoft.Extensions.DependencyInjection;
using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;

namespace Tql.Plugins.GitHub.Categories;

internal class RepositoryMatch
    : IRunnableMatch,
        ISerializableMatch,
        ICopyableMatch,
        ISearchableMatch,
        IHasSearchHint
{
    private readonly RepositoryMatchDto _dto;
    private readonly GitHubApi _api;

    public string Text => _dto.Name;
    public ImageSource Icon => Images.Repository;
    public MatchTypeId TypeId => TypeIds.Repository;

    public string SearchHint => "Find issues";

    public RepositoryMatch(RepositoryMatchDto dto, GitHubApi api)
    {
        _dto = dto;
        _api = api;
    }

    public Task Run(IServiceProvider serviceProvider, Window owner)
    {
        serviceProvider.GetRequiredService<IUI>().OpenUrl(_dto.Url);

        return Task.CompletedTask;
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }

    public Task Copy(IServiceProvider serviceProvider)
    {
        serviceProvider.GetRequiredService<IClipboard>().CopyUri(Text, _dto.Url);

        return Task.CompletedTask;
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

        var client = await _api.GetClient(_dto.ConnectionId);

        var response = await client.Search.SearchIssues(
            new SearchIssuesRequest(text) { Repos = { _dto.Name } }
        );

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(
            p =>
                new IssueMatch(
                    new IssueMatchDto(
                        _dto.ConnectionId,
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
