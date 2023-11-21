using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class GistType(
    IMatchFactory<GistMatch, GistMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<GistMatch, GistMatchDto>(factory)
{
    public override Guid Id => TypeIds.Gist.Id;

    protected override bool IsValid(GistMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
