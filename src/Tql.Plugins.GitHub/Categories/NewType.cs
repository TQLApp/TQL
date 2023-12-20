using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class NewType(
    IMatchFactory<NewMatch, NewMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<NewMatch, NewMatchDto>(factory)
{
    public override Guid Id => TypeIds.New.Id;

    protected override bool IsValid(NewMatchDto dto) =>
        (!dto.Id.HasValue || configurationManager.Configuration.HasConnection(dto.Id.Value))
        && dto.Type != NewMatchType.PullRequest;
}
