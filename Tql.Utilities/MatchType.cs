using System.Text.Json;
using Tql.Abstractions;

namespace Tql.Utilities;

public abstract class MatchType<TMatch, TDto> : IMatchType
    where TMatch : IMatch
{
    private readonly IMatchFactory<TMatch, TDto> _factory;

    public abstract Guid Id { get; }

    protected MatchType(IMatchFactory<TMatch, TDto> factory)
    {
        _factory = factory;
    }

    public IMatch? Deserialize(string json)
    {
        var dto = JsonSerializer.Deserialize<TDto>(json)!;
        if (!IsValid(dto))
            return null;

        return _factory.Create(dto);
    }

    protected virtual bool IsValid(TDto dto) => true;
}
