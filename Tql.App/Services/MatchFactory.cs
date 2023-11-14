using Tql.Abstractions;
using Tql.Utilities;

namespace Tql.App.Services;

internal class MatchFactory<TMatch, TDto> : IMatchFactory<TMatch, TDto>
    where TMatch : IMatch
{
    private readonly Func<TDto, TMatch> _factory;

    public MatchFactory(IServiceProvider serviceProvider)
    {
        _factory = DIUtils.CreateFactory<TDto, TMatch>(serviceProvider);
    }

    public TMatch Create(TDto dto) => _factory(dto);
}
