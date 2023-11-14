using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Demo.Categories;

internal class DemoType : MatchType<DemoMatch, DemoMatchDto>
{
    public override Guid Id => TypeIds.Demo.Id;

    public DemoType(IMatchFactory<DemoMatch, DemoMatchDto> factory)
        : base(factory) { }
}
