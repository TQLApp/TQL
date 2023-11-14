using Tql.Abstractions;
using Tql.Plugins.AzureDevOps.Services;
using Tql.Utilities;

namespace Tql.Plugins.AzureDevOps.Categories;

internal class NewType : MatchType<NewMatch, NewMatchDto>
{
    private readonly ConfigurationManager _configurationManager;

    public override Guid Id => TypeIds.New.Id;

    public NewType(
        IMatchFactory<NewMatch, NewMatchDto> factory,
        ConfigurationManager configurationManager
    )
        : base(factory)
    {
        _configurationManager = configurationManager;
    }

    protected override bool IsValid(NewMatchDto dto) =>
        _configurationManager.Configuration.HasConnection(dto.Url);
}
