using Tql.App.Support;

namespace Tql.App.Search;

internal class SimplifiedString
{
    public string Text { get; }
    public ImmutableArray<int> Positions { get; }

    public SimplifiedString(CharDistribution distribution, string input)
    {
        var positions = ImmutableArray.CreateBuilder<int>(input.Length);

        var sb = StringBuilderCache.Acquire();

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (distribution.Matches(c))
            {
                sb.Append(c);
                positions.Add(i);
            }
        }

        Positions = positions.ToImmutable();
        Text = StringBuilderCache.GetStringAndRelease(sb);
    }

    public TextRange GetRangeFromInput(int offset, int length)
    {
        int start = Positions[offset];
        int end = Positions[offset + length - 1];

        return new TextRange(start, end - start + 1);
    }
}
