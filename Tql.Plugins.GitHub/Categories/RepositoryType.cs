using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class RepositoryType : IMatchType
{
    private readonly GitHubApi _api;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.Repository.Id;

    public RepositoryType(GitHubApi api, ConfigurationManager configurationManager)
    {
        _api = api;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RepositoryMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.ConnectionId))
            return null;

        return new RepositoryMatch(dto, _api);
    }
}
