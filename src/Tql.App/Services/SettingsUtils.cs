using System.Globalization;
using System.Text.RegularExpressions;

namespace Tql.App.Services;

internal static class SettingsUtils
{
    public static Color ParseMainWindowTint(string? value)
    {
        var re = new Regex("^#([a-z0-9]{8})$", RegexOptions.IgnoreCase);

        string tint;
        if (value != null && re.IsMatch(value))
            tint = value;
        else
            tint = Settings.DefaultMainWindowTint;

        var match = re.Match(tint);

        if (!match.Success)
            throw new InvalidOperationException("Default tint is invalid");

        return Color.FromArgb(Parse(0), Parse(2), Parse(4), Parse(6));

        byte Parse(int offset) =>
            (byte)
                int.Parse(
                    match.Groups[1].Value.Substring(offset, 2),
                    NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture
                );
    }
}
