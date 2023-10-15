using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Tql.Utilities;

public class MatchTypeManagerBuilder
{
    private readonly List<Type> _types;

    public MatchTypeManagerBuilder(Assembly assembly)
    {
        _types = (
            from type in assembly.GetTypes()
            where typeof(IMatchType).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract
            select type
        ).ToList();
    }

    public void ConfigureServices(IServiceCollection services)
    {
        foreach (var type in _types)
        {
            services.AddSingleton(type);
        }
    }

    public MatchTypeManager Build(IServiceProvider serviceProvider)
    {
        return new MatchTypeManager(
            _types.Select(p => (IMatchType)serviceProvider.GetRequiredService(p))
        );
    }
}
