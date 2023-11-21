using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class RepositoriesMatch(
    RootItemDto dto,
    GitHubApi api,
    ICache<GitHubData> cache,
    ConfigurationManager configurationManager,
    IMatchFactory<RepositoryMatch, RepositoryMatchDto> factory
) : ISearchableMatch, ISerializableMatch
{
    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.RepositoriesMatch_Label,
            Labels.RepositoriesMatch_MyLabel,
            configurationManager.Configuration,
            dto
        );

    public ImageSource Icon => Images.Repository;
    public MatchTypeId TypeId => TypeIds.Repositories;
    public string SearchHint => Labels.RepositoriesMatch_SearchHint;

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

        var request = new SearchRepositoriesRequest(
            await GitHubUtils.GetSearchPrefix(dto, cache) + text
        );

        if (text.IsWhiteSpace())
        {
            request.SortField = RepoSearchSort.Updated;
            request.Order = SortDirection.Descending;
        }

        var response = await client.Search.SearchRepo(request);

        cancellationToken.ThrowIfCancellationRequested();

        return response
            .Items
            .Select(p => factory.Create(new RepositoryMatchDto(dto.Id, p.FullName, p.HtmlUrl)));
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
