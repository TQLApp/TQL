using Tql.Abstractions;
using Tql.Plugins.Demo.Support;
using Tql.Utilities;

namespace Tql.Plugins.Demo.Categories;

[RootMatchType]
internal class DemoesType(IMatchFactory<DemoesMatch, DemoesMatchDto> factory)
    : MatchType<DemoesMatch, DemoesMatchDto>(factory)
{
    public override Guid Id => TypeIds.Demoes.Id;
}
