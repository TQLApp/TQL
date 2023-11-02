using Tql.Abstractions;
using Tql.Plugins.GitHub.Data;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType(SupportsUserScope = true)]
internal class RepositoriesType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;
    private readonly GitHubApi _api;
    private readonly ICache<GitHubData> _cache;

    public Guid Id => TypeIds.Repositories.Id;

    public RepositoriesType(
        ConfigurationManager configurationManager,
        GitHubApi api,
        ICache<GitHubData> cache
    )
    {
        _configurationManager = configurationManager;
        _api = api;
        _cache = cache;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.HasConnection(dto.Id))
            return null;

        return new RepositoriesMatch(
            MatchUtils.GetMatchLabel(
                Labels.RepositoriesType_Label,
                Labels.RepositoriesType_MyLabel,
                configuration,
                dto
            ),
            dto,
            _api,
            _cache
        );
    }
}
