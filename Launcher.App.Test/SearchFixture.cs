using Launcher.Abstractions;
using Launcher.App.Search;

namespace Launcher.App.Test;

[TestFixture]
public class SearchFixture
{
    [TestCase("a", "abc", ">a<bc")]
    [TestCase("ab", "abc", ">ab<c")]
    [TestCase("ac", "abc", ">a<b>c<")]
    [TestCase("x", "abc", null)]
    public void ExactMatches(string search, string text, string? expected)
    {
        AssertMatch(search, text, expected);
    }

    [TestCase("ab", "abc", ">ab<c", 0)]
    [TestCase("ba", "abc", ">a<bc", 2)]
    [TestCase("dfeg", "abcdefghij", "abc>defg<hij", 0)]
    public void FuzzyMatch(string search, string text, string? expected, int penalty)
    {
        AssertMatch(search, text, expected, penalty);
    }

    [TestCase("ab", "abc", null)]
    [TestCase("ba", "abc", "a")]
    [TestCase("dfeg", "abcdefghij", "def")]
    public void FuzzySearch(string search, string text, string expected)
    {
        var searchResult = GetSearchResult(search, text);

        Assert.AreEqual(expected, searchResult.FuzzyText);
    }

    private void AssertMatch(string search, string text, string? expected, int? penalty = null)
    {
        var searchResult = GetSearchResult(search, text);

        var formatted = FormatSearchResult(searchResult);

        Assert.AreEqual(expected, formatted);
        if (penalty.HasValue)
            Assert.AreEqual(penalty.Value, searchResult.Penalty);
    }

    private static SearchResult GetSearchResult(string search, string text)
    {
        var searchContext = new SearchContext(null!, search, null);

        return searchContext.GetSearchResult(new Match(text, null!, default));
    }

    private string? FormatSearchResult(SearchResult searchResult)
    {
        if (searchResult.Penalty == null)
            return null;

        var result = searchResult.Text;
        if (searchResult.TextMatch == null)
            return result;

        foreach (var range in searchResult.TextMatch.Ranges.Reverse())
        {
            result =
                result.Substring(0, range.Offset)
                + ">"
                + result.Substring(range.Offset, range.Length)
                + "<"
                + result.Substring(range.Offset + range.Length);
        }

        return result;
    }

    private record Match(string Text, IImage Icon, Guid TypeId) : IMatch;
}
