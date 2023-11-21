using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class UserType(
    IMatchFactory<UserMatch, UserMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<UserMatch, UserMatchDto>(factory)
{
    public override Guid Id => TypeIds.User.Id;

    protected override bool IsValid(UserMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
