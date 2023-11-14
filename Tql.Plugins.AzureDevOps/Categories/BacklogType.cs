using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BacklogType : MatchType<BacklogMatch, BacklogMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Backlog.Id;

    public BacklogType(
        IMatchFactory<BacklogMatch, BacklogMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(BacklogMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
