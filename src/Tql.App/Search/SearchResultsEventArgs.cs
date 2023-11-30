namespace Tql.App.Search;

internal class SearchResultsEventArgs(ImmutableArray<SearchResult> results, bool isPreliminary)
    : EventArgs
{
    public ImmutableArray<SearchResult> Results { get; } = results;
    public bool IsPreliminary { get; } = isPreliminary;
}
