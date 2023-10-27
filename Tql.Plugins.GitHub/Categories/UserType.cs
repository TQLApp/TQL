using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class UserType : IMatchType
{
    private readonly GitHubApi _api;
    private readonly ConfigurationManager _configurationManager;

    public Guid Id => TypeIds.User.Id;

    public UserType(GitHubApi api, ConfigurationManager configurationManager)
    {
        _api = api;
        _configurationManager = configurationManager;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<UserMatchDto>(json)!;
        if (!_configurationManager.Configuration.HasConnection(dto.ConnectionId))
            return null;

        return new UserMatch(dto, _api);
    }
}
