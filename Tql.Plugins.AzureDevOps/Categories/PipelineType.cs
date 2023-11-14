using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class PipelineType : MatchType<PipelineMatch, PipelineMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.Pipeline.Id;

    public PipelineType(
        IMatchFactory<PipelineMatch, PipelineMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(PipelineMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
