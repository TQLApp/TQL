using Tql.App.Support;

namespace Tql.App.Search;

internal class CharDistribution
{
    private readonly bool _haveLetters;
    private readonly bool _haveDigits;
    private readonly string _specialChars;

    public CharDistribution(string input)
    {
        var sb = StringBuilderCache.Acquire();

        foreach (char c in input)
        {
            if (char.IsLetter(c))
                _haveLetters = true;
            else if (char.IsDigit(c))
                _haveDigits = true;
            else
                sb.Append(c);
        }

        _specialChars = StringBuilderCache.GetStringAndRelease(sb);
    }

    public bool Matches(char c)
    {
        return _haveLetters && char.IsLetter(c)
            || _haveDigits && char.IsDigit(c)
            || _specialChars.IndexOf(c) != -1;
    }
}
