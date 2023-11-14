using Tql.Abstractions;
using Tql.Plugins.Demo.Support;
using Tql.Utilities;

namespace Tql.Plugins.Demo.Categories;

[RootMatchType]
internal class DemoesType : MatchType<DemoesMatch, DemoesMatchDto>
{
    public override Guid Id => TypeIds.Demoes.Id;

    public DemoesType(IMatchFactory<DemoesMatch, DemoesMatchDto> factory)
        : base(factory) { }
}
