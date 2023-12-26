namespace Tql.App.Search;

internal class SearchResultsEventArgs(
    ImmutableArray<SearchResult> results,
    bool isPreliminary,
    bool isUpdate
) : EventArgs
{
    public ImmutableArray<SearchResult> Results { get; } = results;
    public bool IsPreliminary { get; } = isPreliminary;
    public bool IsUpdated { get; } = isUpdate;
}
