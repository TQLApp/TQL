using Tql.Abstractions;

namespace Tql.Plugins.Demo.Categories;

internal class DemoesMatch(DemoesMatchDto dto, IMatchFactory<DemoMatch, DemoMatchDto> factory)
    : ISearchableMatch,
        ISerializableMatch
{
    public string Text => Labels.DemoesMatch_Label;
    public ImageSource Icon => Images.SpaceShuttle;
    public MatchTypeId TypeId => TypeIds.Demoes;
    public string SearchHint => Labels.DemoesMatch_SearchHint;

    public Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        return Task.FromResult<IEnumerable<IMatch>>(
            context.Filter(new[] { factory.Create(new DemoMatchDto()) })
        );
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(dto);
    }
}

internal record DemoesMatchDto();
