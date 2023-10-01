using System.Globalization;

namespace Launcher.App.Support;

internal static class TextUtils
{
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
}
