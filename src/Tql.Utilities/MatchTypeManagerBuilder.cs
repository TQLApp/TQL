using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Tql.Abstractions;

namespace Tql.Utilities;

/// <summary>
/// Builder for <see cref="IMatchTypeManager"/> objects.
/// </summary>
public class MatchTypeManagerBuilder
{
    /// <summary>
    /// Creates a <see cref="MatchTypeManagerBuilder"/> for the specified
    /// assembly.
    /// </summary>
    /// <remarks>
    /// This method finds all classes in the specified assembly that
    /// implement the <see cref="IMatchType"/> interface and are not
    /// abstract.
    /// </remarks>
    /// <param name="assembly">Assembly to find match types in.</param>
    /// <returns>Builder for a <see cref="IMatchTypeManager"/>.</returns>
    public static MatchTypeManagerBuilder ForAssembly(Assembly assembly)
    {
        var types = (
            from type in assembly.GetTypes()
            where typeof(IMatchType).IsAssignableFrom(type) && type.IsClass && !type.IsAbstract
            select type
        ).ToList();

        return new MatchTypeManagerBuilder(types);
    }

    private readonly List<Type> _types;

    private MatchTypeManagerBuilder(List<Type> types)
    {
        _types = types;
    }

    /// <summary>
    /// Configures the service collection with the discovered match types.
    /// </summary>
    /// <remarks>
    /// Call this method from your <see cref="ITqlPlugin.ConfigureServices(IServiceCollection)"/>
    /// implementation.
    /// </remarks>
    /// <param name="services">Service collection to configure.</param>
    public void ConfigureServices(IServiceCollection services)
    {
        foreach (var type in _types)
        {
            services.AddSingleton(type);
        }
    }

    /// <summary>
    /// Builds a <see cref="IMatchTypeManager"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method builds the match type manager populated with an instance
    /// of all discovered match type implementations.
    /// </para>
    ///
    /// <para>
    /// Call this method from your <see cref="ITqlPlugin.Initialize(IServiceProvider)"/>
    /// method and use the returned <see cref="IMatchTypeManager"/>
    /// to implement the <see cref="ITqlPlugin.DeserializeMatch(Guid, string)"/>
    /// method of your plugin.
    /// </para>
    /// </remarks>
    /// <param name="serviceProvider">Service provider used to instantiate
    /// the match types.</param>
    /// <returns>Initialized <see cref="IMatchTypeManager"/>.</returns>
    public IMatchTypeManager Build(IServiceProvider serviceProvider)
    {
        return new MatchTypeManager(
            _types.Select(p => (IMatchType)serviceProvider.GetRequiredService(p))
        );
    }

    private class MatchTypeManager : IMatchTypeManager
    {
        private readonly Dictionary<Guid, IMatchType> _matchTypes;

        public ImmutableArray<IMatchType> MatchTypes { get; }

        public MatchTypeManager(IEnumerable<IMatchType> matchTypes)
        {
            MatchTypes = matchTypes.ToImmutableArray();

            _matchTypes = MatchTypes.ToDictionary(p => p.Id, p => p);
        }

        public IMatch? Deserialize(Guid typeId, string value)
        {
            if (!_matchTypes.TryGetValue(typeId, out var type))
                return null;

            return type.Deserialize(value);
        }
    }
}
