namespace Launcher.Plugins.GitHub.Support;

internal static class StringExtensions
{
    public static bool IsEmpty(this string self) => string.IsNullOrEmpty(self);

    public static bool IsWhiteSpace(this string self) => string.IsNullOrWhiteSpace(self);
}
