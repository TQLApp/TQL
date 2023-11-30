namespace Tql.Abstractions;

/// <summary>
/// Represents a filtered collection of matches.
/// </summary>
/// <remarks>
/// <para>
/// <see cref="ISearchContext.Filter(IEnumerable{IMatch})"/> returns this
/// interface to filter search results. This interface must be enumerated
/// asynchronously and will throw an exception if it's enumerated
/// synchronously.
/// </para>
///
/// <para>
/// Use the take method on this interface if you only need a subset of
/// the results.
/// </para>
/// </remarks>
public interface IFilteredAsyncEnumerable : IAsyncEnumerable<IMatch>, IEnumerable<IMatch>
{
    /// <summary>
    /// Gets the top <paramref name="count"/> results from the filtered search results.
    /// </summary>
    /// <param name="count">Number of search results to return.</param>
    /// <returns>Top <paramref name="count"/> filtered search results.</returns>
    IFilteredAsyncEnumerable Take(int count);
}
