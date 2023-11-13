using Microsoft.Win32;
using Tql.App.Themes;

namespace Tql.App.Support;

internal static class ThemeManager
{
    public static Theme CurrentTheme { get; private set; } = GetSystemTheme();

    public static void SetTheme(Theme theme)
    {
        if (theme == Theme.System)
            theme = GetSystemTheme();

        CurrentTheme = theme;

        ThemesController.SetTheme(theme == Theme.Light ? ThemeType.LightTheme : ThemeType.SoftDark);
    }

    public static Theme GetSystemTheme()
    {
        using var key = Registry.CurrentUser.OpenSubKey(
            @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize"
        );

        var isLight = (key?.GetValue("AppsUseLightTheme") as int?) != 0;

        return isLight ? Theme.Light : Theme.Dark;
    }

    public static Theme ParseTheme(string? value)
    {
        if (value != null && Enum.TryParse<Theme>(value, out var theme))
            return theme;

        return Theme.System;
    }
}
