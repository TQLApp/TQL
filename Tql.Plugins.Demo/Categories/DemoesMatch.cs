using Tql.Abstractions;

namespace Tql.Plugins.Demo.Categories;

internal class DemoesMatch : ISearchableMatch, ISerializableMatch
{
    public string Text => "Demo Plugin";
    public ImageSource Icon => Images.SpaceShuttle;
    public MatchTypeId TypeId => TypeIds.Demoes;

    public Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult(context.Filter(new[] { new DemoMatch() }));
    }

    public string Serialize()
    {
        return "{}";
    }
}
