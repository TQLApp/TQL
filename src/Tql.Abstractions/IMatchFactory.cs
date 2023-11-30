namespace Tql.Abstractions;

/// <summary>
/// Represents a factory for <see cref="IMatch"/> instances.
/// </summary>
/// <remarks>
/// <para>
/// The match factory service is a way to allow your plugin to better
/// integrate with the dependency injection system.
/// </para>
///
/// <para>
/// Every match is
/// expected to have an accompanying DTO object. This DTO object
/// will be an argument in the constructor of the match. The match
/// however may need more services to do its work. The purpose of
/// the match factory class is to "close over" all arguments of the
/// match constructor other than the DTO object.
/// </para>
///
/// <para>
/// <see cref="Create(TDto)"/> calls the constructor of the match class
/// with all services it requests and the DTO object. The advantage
/// of this is that you're not exposing these dependencies anymore.
/// This greatly simplifies implementing searchable matches. The searchable
/// match just takes a match factory of the match type it's searching
/// and doesn't have to care about its dependencies.
/// </para>
///
/// <para>
/// Match factory uses IL code generation to optimize its implementation.
/// </para>
/// </remarks>
/// <typeparam name="TMatch">Type of the match.</typeparam>
/// <typeparam name="TDto">Type of the match's DTO object.</typeparam>
public interface IMatchFactory<out TMatch, in TDto>
    where TMatch : IMatch
{
    /// <summary>
    /// Create a new instance of <typeparamref name="TMatch"/>.
    /// </summary>
    /// <param name="dto">DTO object to pass to the match.</param>
    /// <returns>New match instance.</returns>
    TMatch Create(TDto dto);
}
