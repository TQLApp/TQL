using Tql.Abstractions;
using Tql.Plugins.GitHub.Services;
using Tql.Utilities;

namespace Tql.Plugins.GitHub.Categories;

internal class GistType : MatchType<GistMatch, GistMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Gist.Id;

    public GistType(
        IMatchFactory<GistMatch, GistMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(GistMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.ConnectionId);
}
