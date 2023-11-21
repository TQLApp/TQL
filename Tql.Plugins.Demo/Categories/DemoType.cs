using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Demo.Categories;

internal class DemoType(IMatchFactory<DemoMatch, DemoMatchDto> factory)
    : MatchType<DemoMatch, DemoMatchDto>(factory)
{
    public override Guid Id => TypeIds.Demo.Id;
}
