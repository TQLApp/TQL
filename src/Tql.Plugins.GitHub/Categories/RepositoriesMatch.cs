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
        if (dto.Scope == RootItemScope.User)
        {
            var data = await cache.Get();
            var connection = data.GetConnection(dto.Id);

            if (text.IsWhiteSpace())
            {
                return connection
                    .Repositories.OrderByDescending(p => p.UpdatedAt)
                    .Select(CreateMatch);
            }

            return context.Filter(connection.Repositories.Select(CreateMatch));

            RepositoryMatch CreateMatch(GitHubRepository repository)
            {
                return factory.Create(
                    new RepositoryMatchDto(
                        dto.Id,
                        repository.Owner,
                        repository.Name,
                        repository.HtmlUrl
                    )
                );
            }
        }

        if (text.IsWhiteSpace())
            return Array.Empty<IMatch>();

        await context.DebounceDelay(cancellationToken);

        var client = await api.GetClient(dto.Id);

        var request = new SearchRepositoriesRequest(
            await GitHubUtils.GetSearchPrefix(dto, cache) + text
        );

        var response = await client.Search.SearchRepo(request);

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(p =>
            factory.Create(new RepositoryMatchDto(dto.Id, p.Owner.Login, p.Name, p.HtmlUrl))
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}
