using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class UserType : MatchType<UserMatch, UserMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.User.Id;

    public UserType(
        IMatchFactory<UserMatch, UserMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(UserMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
