using Launcher.Abstractions;

namespace Launcher.App.Search;

/// <remarks>
/// Text is cached because it's read often. This simplifies the implementation
/// of IMatch by plugin authors because they don't have to worry about
/// caching Text themselves.
/// </remarks>
internal record SearchResult(
    IMatch Match,
    string Text,
    string SimpleText,
    string? FuzzyText,
    TextMatch? TextMatch,
    long? HistoryId,
    DateTime? LastAccessed,
    int? Penalty
);
