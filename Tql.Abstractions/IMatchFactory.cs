namespace Tql.Abstractions;

public interface IMatchFactory<out TMatch, in TDto>
    where TMatch : IMatch
{
    TMatch Create(TDto dto);
}
