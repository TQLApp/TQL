using Tql.Abstractions;

namespace Tql.Plugins.Demo.Categories;

internal class DemoesMatch : ISearchableMatch, ISerializableMatch
{
    public string Text => Labels.DemoesMatch_Label;
    public ImageSource Icon => Images.SpaceShuttle;
    public MatchTypeId TypeId => TypeIds.Demoes;

    public Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        return context.FilterAsync(new[] { new DemoMatch() });
    }

    public string Serialize()
    {
        return "{}";
    }
}
