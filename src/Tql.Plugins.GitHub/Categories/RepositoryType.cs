using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class RepositoryType(
    IMatchFactory<RepositoryMatch, RepositoryMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<RepositoryMatch, RepositoryMatchDto>(factory)
{
    public override Guid Id => TypeIds.Repository.Id;

    protected override bool IsValid(RepositoryMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
