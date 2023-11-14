using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Plugins.GitHub.Support;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

[RootMatchType]
internal class UsersType : MatchType<UsersMatch, RootItemDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Users.Id;

    public UsersType(
        IMatchFactory<UsersMatch, RootItemDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(RootItemDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Id);
}
