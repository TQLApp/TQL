namespace Tql.App.Search;

internal static class FuzzyTextMatching
{
    public static (string? FuzzyLabel, int Distance) Match(
        Levenshtein levenshtein,
        string search,
        CharDistribution distribution,
        string simpleText,
        string lowerSimpleText
    )
    {
        var simplifiedText = new SimplifiedString(distribution, lowerSimpleText);
        if (simplifiedText.Text.Length > 0)
        {
            var (distance, offset, length) = Calculate(levenshtein, search, simplifiedText);
            if (distance <= 3)
            {
                var textRange = simplifiedText.GetRangeFromInput(offset, length);

                return (simpleText.Substring(textRange.Offset, textRange.Length), distance);
            }
        }

        return (null, -1);
    }

    private static (int Distance, int Offset, int Length) Calculate(
        Levenshtein levenshtein,
        string search,
        SimplifiedString itemLabel
    )
    {
        string simpleItemLabel = itemLabel.Text;

        if (simpleItemLabel.Length < search.Length)
            return (levenshtein.DistanceFrom(simpleItemLabel), 0, simpleItemLabel.Length);

        int result = int.MaxValue;
        int offset = -1;
        int length = -1;
        int realLength = -1;

        int max = Math.Max(simpleItemLabel.Length - search.Length + 1, 0);

        // Walk the start of itemLabel.

        for (int i = 0; i <= max; i++)
        {
            // Vary the string we're matching on between one shorter than
            // the search string, the same length, and one longer.

            for (int j = -1; j <= 1; j++)
            {
                int itemLabelSubstringLength = search.Length + j;
                if (
                    itemLabelSubstringLength <= 0
                    || i + itemLabelSubstringLength > simpleItemLabel.Length
                )
                    break;

                int distance = levenshtein.DistanceFrom(simpleItemLabel, i, search.Length + j);

                // PERFORMANCE
                // We're not interested in high distances. The max distance we include
                // is 3. If this distance goes over 3, that means it won't be included.
                // However, if it goes over 5, the other iterations of this inner loop
                // won't have any use, because they won't go below 3 (likely).

                if (distance > Constants.MaxDistance + 2)
                {
                    // PERFORMANCE
                    // If the distance is very high, we can assume that the next outer
                    // loop iteration will not result in a match. The heuristic below
                    // is based on this. If the distance is 6, we skip one outer loop
                    // iteration. On 9, we skip 2, etc. The assumption is that if
                    // we're 6 away now, the next loop iteration won't get us under 3.

                    i += distance / Constants.MaxDistance - 1;
                    break;
                }

                int thisRealLength = itemLabel
                    .GetRangeFromInput(i, itemLabelSubstringLength)
                    .Length;

                if (distance < result || distance == result && thisRealLength < realLength)
                {
                    result = distance;
                    offset = i;
                    length = itemLabelSubstringLength;
                    realLength = thisRealLength;
                }
            }
        }

        return (result, offset, length);
    }
}
