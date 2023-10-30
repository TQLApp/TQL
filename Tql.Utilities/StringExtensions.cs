namespace Tql.Utilities;

public static class StringExtensions
{
    public static bool IsEmpty(this string self) => string.IsNullOrEmpty(self);

    public static bool IsWhiteSpace(this string self) => string.IsNullOrWhiteSpace(self);

    public static bool Contains(this string self, string search, StringComparison comparison) =>
        self.IndexOf(search, comparison) != -1;
}
