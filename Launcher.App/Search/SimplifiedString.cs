using Launcher.App.Support;

namespace Launcher.App.Search;

internal class SimplifiedString
{
    private readonly List<int> _positions;

    public string Text { get; }

    public SimplifiedString(CharDistribution distribution, string input)
    {
        _positions = new List<int>(input.Length);

        var sb = StringBuilderCache.Acquire();

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (distribution.Matches(c))
            {
                sb.Append(c);
                _positions.Add(i);
            }
        }

        Text = StringBuilderCache.GetStringAndRelease(sb);
    }

    public TextRange GetRangeFromInput(int offset, int length)
    {
        int start = _positions[offset];
        int end = _positions[offset + length - 1];

        return new TextRange(start, end - start + 1);
    }
}
