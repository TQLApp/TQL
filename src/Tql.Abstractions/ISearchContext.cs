namespace Tql.Abstractions;

/// <summary>
/// Represents the search context to help you interact with
/// TQL when doing searches.
/// </summary>
public interface ISearchContext
{
    /// <summary>
    /// Gets the service provider.
    /// </summary>
    /// <remarks>
    /// The preferred pattern is to use DI to have required services injected
    /// into the match class. However, under certain circumstances this may not
    /// be feasible. In that case, this service provider can be used to resolve
    /// any dependencies.
    /// </remarks>
    IServiceProvider ServiceProvider { get; }

    /// <summary>
    /// Gets the context map.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The context map is used to allow you to temporarily store data.
    /// </para>
    ///
    /// <para>
    /// The primary use case is to allow you to cache data for a very short period,
    /// i.e. as long as the user has the search window open. Every time the user
    /// starts searching, the context map is reset.
    /// </para>
    ///
    /// <para>
    /// In general you should use the caching infrastructure to cache data.
    /// See <see cref="ICacheManager{T}"/> for more information on this. However,
    /// some APIs may not play nice with this. If you have an API that does not
    /// implement filter capabilities, but that does change often enough so
    /// that you don't want to cache it, you can call the API and store the results
    /// here. See SearchContextExtensions.GetCachedData in the utilities library
    /// for a helper method to simplify implementing this pattern.
    /// </para>
    /// </remarks>
    IDictionary<string, object> Context { get; }

    /// <summary>
    /// Gets a task that delays for the configured debounce interval.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is used to allow you to implement debounce when implementing
    /// the <see cref="ISearchableMatch.Search(ISearchContext, string, CancellationToken)"/>
    /// method.
    /// </para>
    ///
    /// <para>
    /// The following example shows how this method is normally used:
    /// </para>
    ///
    /// <code><![CDATA[
    /// public async Task<IEnumerable<IMatch>> Search(ISearchContext context, string text, CancellationToken cancellationToken)
    /// {
    ///     if (string.IsNullOrEmpty(text))
    ///         return Array.Empty<IMatch>();
    ///
    ///     await context.DebounceDelay(cancellationToken);
    ///
    ///     // Perform search
    /// }
    /// ]]></code>
    /// </remarks>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Task representing the debounce delay.</returns>
    Task DebounceDelay(CancellationToken cancellationToken);

    /// <summary>
    /// Filters and sorts search results.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Usually search results are retrieved in one of two ways: from
    /// cache or on the fly.
    /// </para>
    ///
    /// <para>
    /// If search results are fetched on the fly,
    /// and the API used to retrieve the search results has filter
    /// capabilities, the assumption is that we should respect the
    /// matched items and order the server sends us. In that case,
    /// you would return search results from the API as is in the
    /// <see cref="ISearchableMatch.Search(ISearchContext, string, CancellationToken)"/>
    /// method.
    /// </para>
    ///
    /// <para>
    /// However, if the API doesn't have filter capabilities, or
    /// the search results were retrieved from cache, we need to
    /// filter and sort them before showing them to the user. This
    /// is the intended purpose of the filter method.
    /// </para>
    ///
    /// <para>
    /// The filter method takes the full list of search results and
    /// filters them according to the current search string. This is
    /// also where fuzzy matching and the ordering algorithms are
    /// implemented.
    /// </para>
    /// </remarks>
    /// <param name="matches">Search results to filter and sort.</param>
    /// <returns>Filtered and sorted search results.</returns>
    IFilteredAsyncEnumerable Filter(IEnumerable<IMatch> matches);

    /// <summary>
    /// Suppresses preliminary results.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Preliminary results are search results that TQL shows to the
    /// user if it detects that a category is getting search results
    /// asynchronously, e.g. from a server. These preliminary search
    /// results come from the users history. The reason for showing this
    /// is that this allows the user to select a search result from the
    /// history instead of having to wait for the search to complete.
    /// </para>
    ///
    /// <para>
    /// This behavior may not be desirable. The primary use case of this
    /// method is to search on work items by ID, but this method
    /// can be useful in similar scenarios. The idea is that if a user
    /// is searching a work item by ID, he wants to be able to press
    /// enter after typing in the work item ID. If TQL shows preliminary
    /// results, the user will pick a search result from the history,
    /// which will likely be the wrong one. In such scenarios you
    /// can suppress showing preliminary search results.
    /// </para>
    ///
    /// <para>
    /// The intended usage of this method is as follows:
    /// </para>
    ///
    /// <code><![CDATA[
    /// public async Task<IEnumerable<IMatch>> Search(ISearchContext context, string text, CancellationToken cancellationToken)
    /// {
    ///     if (string.IsNullOrEmpty(text))
    ///         return Array.Empty<IMatch>();
    ///
    ///     var isWorkItemId = int.TryParse(text, out var workItemId);
    ///     if (isWorkItemId)
    ///     {
    ///         //
    ///         // The SuppressPreliminaryResults method must be called before any
    ///         // async operation (like the debounce delay). TQL will show
    ///         // preliminary results immediately when the Search method returns
    ///         // with an incomplete Task. Preliminary search results can only
    ///         // be suppressed if the method is called before the search method
    ///         // first returns (so before the first await keyword).
    ///         //
    ///
    ///         context.SuppressPreliminaryResults();
    ///     }
    ///
    ///     await context.DebounceDelay(cancellationToken);
    ///
    ///     if (isWorkItemId)
    ///         // Find the work item by ID.
    ///     else
    ///         // Find work items by doing a full text search.
    /// }
    /// ]]></code>
    /// </remarks>
    void SuppressPreliminaryResults();
}
