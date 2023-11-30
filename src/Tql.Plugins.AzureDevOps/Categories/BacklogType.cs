using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class BacklogType(
    IMatchFactory<BacklogMatch, BacklogMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<BacklogMatch, BacklogMatchDto>(factory)
{
    public override Guid Id => TypeIds.Backlog.Id;

    protected override bool IsValid(BacklogMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
