using System.Globalization;
using System.Text.RegularExpressions;

namespace Tql.App.Support;

internal static class TextUtils
{
    private static readonly Regex IsWordCharacterRe = new("^\\w$", RegexOptions.Compiled);

    // From https://stackoverflow.com/questions/249087/how-do-i-remove-diacritics-accents-from-a-string-in-net

    public static string RemoveDiacritics(string text)
    {
        text = text.Normalize(NormalizationForm.FormD);

        var sb = StringBuilderCache.Acquire(text.Length);

        foreach (var c in text)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        return StringBuilderCache.GetStringAndRelease(sb).Normalize(NormalizationForm.FormC);
    }

    public static bool IsWordBoundary(string text, int offset)
    {
        if (offset == 0 || offset == text.Length)
            return true;

        bool leftWordCharacter = IsWordCharacterRe.IsMatch(text.Substring(offset - 1, 1));
        bool rightWordCharacter = IsWordCharacterRe.IsMatch(text.Substring(offset, 1));

        return leftWordCharacter != rightWordCharacter;
    }
}
