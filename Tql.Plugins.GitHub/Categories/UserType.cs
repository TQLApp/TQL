using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class UserType : IMatchType
{
    private readonly GitHubApi _api;

    public Guid Id => TypeIds.User.Id;

    public UserType(GitHubApi api)
    {
        _api = api;
    }

    public IMatch Deserialize(string json)
    {
        return new UserMatch(JsonSerializer.Deserialize<UserMatchDto>(json)!, _api);
    }
}
