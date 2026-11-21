using Tql.Abstractions;
using Tql.Plugins.YouTrack.Services;
using Tql.Utilities;

namespace Tql.Plugins.YouTrack.Categories;

internal class NewType(
    IMatchFactory<NewMatch, NewMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<NewMatch, NewMatchDto>(factory)
{
    public override Guid Id => TypeIds.New.Id;

    protected override bool IsValid(NewMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
