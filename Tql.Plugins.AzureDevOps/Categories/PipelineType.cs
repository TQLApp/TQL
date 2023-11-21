using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class PipelineType(
    IMatchFactory<PipelineMatch, PipelineMatchDto> factory,
    ConfigurationManager configurationManager
) : MatchType<PipelineMatch, PipelineMatchDto>(factory)
{
    public override Guid Id => TypeIds.Pipeline.Id;

    protected override bool IsValid(PipelineMatchDto dto) =>
        configurationManager.Configuration.HasConnection(dto.Url);
}
