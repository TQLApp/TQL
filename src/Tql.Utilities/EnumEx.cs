namespace Tql.Utilities;

/// <summary>
/// Utility methods work working with <c>enum</c>'s.
/// </summary>
public static class EnumEx
{
    /// <summary>
    /// Gets all values of an enum.
    /// </summary>
    /// <typeparam name="T">Type of the enum.</typeparam>
    /// <returns>All values of the enum.</returns>
    public static IEnumerable<T> GetValues<T>() => Enum.GetValues(typeof(T)).Cast<T>();
}
