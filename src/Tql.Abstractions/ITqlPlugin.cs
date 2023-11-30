using Microsoft.Extensions.DependencyInjection;

namespace Tql.Abstractions;

/// <summary>
/// Represents a TQL plugin.
/// </summary>
/// <remarks>
/// <para>
/// This interface is the entry point for TQL into your plugin. You need to
/// implement this interface a public class with a parameter less constructor.
/// You also have to add the <see cref="TqlPluginAttribute"/> attribute to
/// this class. TQL automatically takes care of loading and discovering your
/// plugins if you follow this pattern.
/// </para>
///
/// <para>
/// See the plugin documentation for more detailed information
/// on how to write a plugin.
/// </para>
/// </remarks>
public interface ITqlPlugin
{
    /// <summary>
    /// Gets the ID of the plugin.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the title of the plugin.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Configures services for the plugin.
    /// </summary>
    /// <remarks>
    /// TQL uses Microsoft Dependency Injection. This method is called during
    /// initialization of the app to allow you to register your own
    /// services.
    /// </remarks>
    /// <param name="services">Service collection used to register your own services.</param>
    void ConfigureServices(IServiceCollection services);

    /// <summary>
    /// Initializes the plugin.
    /// </summary>
    /// <remarks>
    /// This method is called after the service container has been fully built.
    /// </remarks>
    /// <param name="serviceProvider">Service provider.</param>
    void Initialize(IServiceProvider serviceProvider);

    /// <summary>
    /// Deserialize a match from JSON.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called when TQL needs to deserialize a match from text,
    /// e.g. from the history. If the match cannot be deserialize, either
    /// because the type doesn't exist (anymore), a connection has been removed
    /// of the serialized text is otherwise invalid, return <c>null</c> from
    /// this method. This signals TQL that the serialized text is invalid
    /// and that it should be removed.
    /// </para>
    ///
    /// <para>
    /// The MatchTypeManager class in the utilities library provides a base
    /// implementation for managing deserialization of matches.
    /// </para>
    /// </remarks>
    /// <param name="typeId">ID of the match type.</param>
    /// <param name="value">Serialized match.</param>
    /// <returns>Deserialized match.</returns>
    IMatch? DeserializeMatch(Guid typeId, string value);

    /// <summary>
    /// Gets the root matches of the plugin.
    /// </summary>
    /// <remarks>
    /// This method returns the root list of matches that your plugin implements.
    /// Almost certainly these matches should implement the
    /// <see cref="ISearchableMatch"/> and <see cref="ISerializableMatch"/>
    /// interfaces. Refer to those interfaces for more information.
    /// </remarks>
    /// <returns>Root matches of your plugin.</returns>
    IEnumerable<IMatch> GetMatches();

    /// <summary>
    /// Gets configuration pages of the plugin.
    /// </summary>
    /// <remarks>
    /// If you need to expose configuration to the user, implement these as
    /// WPF controls and return them from this method. TQL will show these
    /// pages in the configuration window.
    /// </remarks>
    /// <returns>Configuration pages of the plugin.</returns>
    IEnumerable<IConfigurationPage> GetConfigurationPages();
}
