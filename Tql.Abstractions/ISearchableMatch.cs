namespace Tql.Abstractions;

/// <summary>
/// Represents a searchable match.
/// </summary>
/// <remarks>
/// Implement this interface on your match class if the match represents
/// something that can be search. Examples are all JIRA issues, but also
/// issues within a specific JIRA board or an Azure DevOps backlog.
/// A bullet list icon will be shown next to the search result to indicate
/// that the search result is a category.
/// </remarks>
public interface ISearchableMatch : IMatch
{
    /// <summary>
    /// Gets the search hint to show in the search box.
    /// </summary>
    /// <remarks>
    /// Setting the search hint helps the user understand how to use your
    /// search category.
    /// </remarks>
    string SearchHint { get; }

    /// <summary>
    /// Searches the category.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the user first enters the category, a search will be started
    /// with an empty text. This can be used to present the user with a list
    /// of all items in the category, especially if the contents of the
    /// category is cached. You can of course also start an online search
    /// to get all items in the category from the server, but you should
    /// only do this if this has value to the user.
    /// </para>
    ///
    /// <para>
    /// Whenever the search text changes, TQL will abort the previous search
    /// and start a new one. The previous search will be aborted using the
    /// provided cancellation token.
    /// </para>
    ///
    /// <para>
    /// TQL does not implement a debounce! Instead, you're expected to call
    /// <see cref="ISearchContext.DebounceDelay(CancellationToken)"/> to
    /// implement debounce.
    /// </para>
    ///
    /// <para>
    /// The search context provides some features to help you interact with
    /// TQL to provide the best user experience. See <see cref="ISearchContext"/>
    /// for more information on this.
    /// </para>
    /// </remarks>
    /// <param name="context">Context to interact with TQL.</param>
    /// <param name="text">Text to search.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Search results.</returns>
    Task<IEnumerable<IMatch>> Search(
        ISearchContext context,
        string text,
        CancellationToken cancellationToken
    );
}
