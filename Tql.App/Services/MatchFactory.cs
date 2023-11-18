using Tql.Abstractions;
using Expression = System.Linq.Expressions.Expression;

namespace Tql.App.Services;

internal class MatchFactory<TMatch, TDto> : IMatchFactory<TMatch, TDto>
    where TMatch : IMatch
{
    private readonly Func<TDto, TMatch> _factory;

    public MatchFactory(IServiceProvider serviceProvider)
    {
        _factory = CreateFactory(serviceProvider);
    }

    //
    // This method creates a factory for a type where a single argument
    // is provided into the factory, and the rest of the arguments must
    // be resolvable using the service provider. These resolved arguments
    // are "closed over", meaning that they're baked into the lambda.
    //
    // This method is used to be able to make generic match type
    // implementations, and is actually (slightly) faster than tracking
    // the arguments manually (in fields) and instantiating the match
    // object manually.
    //
    public static Func<TDto, TMatch> CreateFactory(IServiceProvider serviceProvider)
    {
        var constructorInfos = typeof(TMatch).GetConstructors();
        if (constructorInfos.Length != 1)
        {
            throw new InvalidOperationException(
                $"Match type '{typeof(TMatch)}' requires exactly one constructor"
            );
        }

        var constructorInfo = constructorInfos[0];
        var parameterInfos = constructorInfo.GetParameters();

        var parameterExpression = Expression.Parameter(typeof(TDto));
        var expressions = new List<Expression>();

        foreach (var parameterInfo in parameterInfos)
        {
            if (parameterInfo.ParameterType == typeof(TDto))
            {
                expressions.Add(parameterExpression);
            }
            else
            {
                var service = serviceProvider.GetService(parameterInfo.ParameterType);
                if (service == null)
                {
                    throw new InvalidOperationException(
                        $"Cannot resolve service of type '{parameterInfo.ParameterType}' for match type '{typeof(TMatch)}'"
                    );
                }

                expressions.Add(Expression.Constant(service, parameterInfo.ParameterType));
            }
        }

        var newExpression = Expression.New(constructorInfo, expressions);

        return Expression.Lambda<Func<TDto, TMatch>>(newExpression, parameterExpression).Compile();
    }

    public TMatch Create(TDto dto) => _factory(dto);
}
