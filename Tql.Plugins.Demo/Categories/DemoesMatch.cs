using System.Text.Json;
using Tql.Abstractions;

namespace Tql.Plugins.Demo.Categories;

internal class DemoesMatch : ISearchableMatch, ISerializableMatch
{
    private readonly DemoesMatchDto _dto;
    private readonly IMatchFactory<DemoMatch, DemoMatchDto> _factory;

    public string Text => Labels.DemoesMatch_Label;
    public ImageSource Icon => Images.SpaceShuttle;
    public MatchTypeId TypeId => TypeIds.Demoes;

    public DemoesMatch(DemoesMatchDto dto, IMatchFactory<DemoMatch, DemoMatchDto> factory)
    {
        _dto = dto;
        _factory = factory;
    }

    public Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    )
    {
        return context.FilterAsync(new[] { _factory.Create(new DemoMatchDto()) });
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(_dto);
    }
}

internal record DemoesMatchDto();
