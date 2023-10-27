using Tql.Abstractions;
using Tql.Plugins.Demo.Support;
using Tql.Utilities;

namespace Tql.Plugins.Demo.Categories;

[RootMatchType]
internal class DemoesType : IMatchType
{
    public Guid Id => TypeIds.Demoes.Id;

    public IMatch Deserialize(string json)
    {
        return new DemoesMatch();
    }
}
