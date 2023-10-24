using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType]
internal class UsersType : IMatchType
{
    private readonly ConfigurationManager _configurationManager;
    private readonly GitHubApi _api;

    public Guid Id => TypeIds.Users.Id;

    public UsersType(ConfigurationManager configurationManager, GitHubApi api)
    {
        _configurationManager = configurationManager;
        _api = api;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<RootItemDto>(json)!;
        var configuration = _configurationManager.Configuration;

        if (!configuration.Connections.Any(p => p.Id == dto.Id))
            return null;

        return new UsersMatch(
            MatchUtils.GetMatchLabel("GitHub User", configuration, dto),
            dto,
            _api
        );
    }
}
