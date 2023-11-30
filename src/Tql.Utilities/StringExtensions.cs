namespace Tql.Utilities;

/// <summary>
/// Utility methods for working with strings.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Extension method for the <see cref="String.IsNullOrEmpty(string)"/> method.
    /// </summary>
    /// <param name="self">String to check whether it's empty.</param>
    /// <returns>Whether the string is empty.</returns>
    public static bool IsEmpty(this string self) => string.IsNullOrEmpty(self);

    /// <summary>
    /// Extension method for the <see cref="String.IsNullOrWhiteSpace(string)"/> method.
    /// </summary>
    /// <param name="self">String to check whether it's all white space.</param>
    /// <returns>Whether the string is all white space.</returns>
    public static bool IsWhiteSpace(this string self) => string.IsNullOrWhiteSpace(self);

    /// <summary>
    /// Helper method to do "contains" searches using a <see cref="StringComparison"/>.
    /// </summary>
    /// <remarks>
    /// This method delegates to <see cref="String.IndexOf(string, StringComparison)"/>
    /// and checks whether result of that method returns <c>-1</c>.
    /// </remarks>
    /// <param name="self">String to search.</param>
    /// <param name="search">What to search for.</param>
    /// <param name="comparison">Comparison type.</param>
    /// <returns>Whether the string contains the search using the specified comparison.</returns>
    public static bool Contains(this string self, string search, StringComparison comparison) =>
        self.IndexOf(search, comparison) != -1;
}
