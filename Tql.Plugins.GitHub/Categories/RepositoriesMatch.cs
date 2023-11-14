using Octokit;
using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class RepositoriesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly RootItemDto _dto;
    private readonly GitHubApi _api;
    private readonly ICache<GitHubData> _cache;
    private readonly ConfigurationManager _configurationManager;
    private readonly IMatchFactory<RepositoryMatch, RepositoryMatchDto> _factory;

    public string Text =>
        MatchUtils.GetMatchLabel(
            Labels.RepositoriesType_Label,
            Labels.RepositoriesType_MyLabel,
            _configurationManager.Configuration,
            _dto
        );

    public ImageSource Icon => Images.Repository;
    public MatchTypeId TypeId => TypeIds.Repositories;

    public RepositoriesMatch(
        RootItemDto dto,
        GitHubApi api,
        ICache<GitHubData> cache,
        ConfigurationManager configurationManager,
        IMatchFactory<RepositoryMatch, RepositoryMatchDto> factory
    )
    {
        _dto = dto;
        _api = api;
        _cache = cache;
        _configurationManager = configurationManager;
        _factory = factory;
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

        var request = new SearchRepositoriesRequest(
            await GitHubUtils.GetSearchPrefix(_dto, _cache) + text
        );

        if (text.IsWhiteSpace())
        {
            request.SortField = RepoSearchSort.Updated;
            request.Order = SortDirection.Descending;
        }

        var response = await client.Search.SearchRepo(request);

        cancellationToken.ThrowIfCancellationRequested();

        return response.Items.Select(
            p => _factory.Create(new RepositoryMatchDto(_dto.Id, p.FullName, p.HtmlUrl))
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}
