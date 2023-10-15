using Tql.App.Support;

namespace Tql.App.Search;

internal static class TextMatching
{
    public static TextMatch? Match(string search, string item, CharDistribution distribution)
    {
        if (search.Length == 0)
            return new TextMatch(ImmutableArray<TextRange>.Empty);

        var simplifiedText = new SimplifiedString(distribution, item);

        var shortest = Find(search, simplifiedText.Text);
        if (shortest == null)
            return null;

        // Now translate the matches back using the simplified string mapper.

        return new TextMatch(
            shortest
                .Select(p => simplifiedText.GetRangeFromInput(p.Offset, p.Length))
                .ToImmutableArray()
        );
    }

    public static IEnumerable<TextRange>? Find(string search, string item)
    {
        if (search.IsEmpty())
            return Array.Empty<TextRange>();

        List<TextRange>? shortest = null;

        foreach (var result in FindAll(search.AsSpan(), item.AsSpan()))
        {
            if (result.Count == 1)
                return result;
            if (shortest == null || result.Count < shortest.Count)
                shortest = result;
        }

        return shortest;
    }

    private static List<List<TextRange>> FindAll(ReadOnlySpan<char> search, ReadOnlySpan<char> item)
    {
        var results = new List<List<TextRange>>();
        int start = 0;

        while (true)
        {
            var match = FindSingle(search, item, start);
            if (match == null)
                break;

            if (match.Value.Length == search.Length)
            {
                results.Add(new List<TextRange> { match.Value });
                break;
            }

            var subSearch = search.Slice(match.Value.Length);
            int offset = match.Value.Offset + match.Value.Length + 1;
            var subItem = item.Slice(offset);

            foreach (var remainder in FindAll(subSearch, subItem))
            {
                for (int i = 0; i < remainder.Count; i++)
                {
                    remainder[i] = new TextRange(remainder[i].Offset + offset, remainder[i].Length);
                }

                remainder.Insert(0, match.Value);

                results.Add(remainder);
            }

            start = match.Value.Offset + 1;
        }

        return results;
    }

    private static TextRange? FindSingle(
        ReadOnlySpan<char> search,
        ReadOnlySpan<char> item,
        int start
    )
    {
        int offset = 0;
        int lastStart = -1;

        for (var i = start; i < item.Length; i++)
        {
            if (search[offset] == item[i])
            {
                if (lastStart == -1)
                    lastStart = i;
                offset++;
                if (offset >= search.Length)
                    return new TextRange(lastStart, i + 1 - lastStart);
            }
            else if (lastStart != -1)
            {
                return new TextRange(lastStart, i - lastStart);
            }
        }

        return null;
    }
}

internal record TextMatch(ImmutableArray<TextRange> Ranges);

internal record struct TextRange(int Offset, int Length);
