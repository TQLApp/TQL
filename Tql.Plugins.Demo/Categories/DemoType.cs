using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.Plugins.Demo.Categories;

internal class DemoType : IMatchType
{
    public Guid Id => TypeIds.Demo.Id;

    public IMatch Deserialize(string json)
    {
        return new DemoMatch();
    }
}
