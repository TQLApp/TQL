using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType]
internal class UsersType(
    IMatchFactory<UsersMatch, RootItemDto> factory,
    ConfigurationManager configurationManager
) : MatchType<UsersMatch, RootItemDto>(factory)
{
    public override Guid Id => TypeIds.Users.Id;

    protected override bool IsValid(RootItemDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Id);
}
